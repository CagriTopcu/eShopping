using MassTransit;
using Microsoft.Extensions.Logging;
using Order.Application.Sagas.Activities;
using Shared.BuildingBlocks.Observability;
using Shared.Contracts.Commands.Payment;
using Shared.Contracts.Commands.Stock;
using Shared.Contracts.Events.Orders;
using Shared.Contracts.Events.Payment;
using Shared.Contracts.Events.Stock;
using System.Text.Json;

namespace Order.Application.Sagas;

public sealed class OrderSaga : MassTransitStateMachine<OrderSagaState>
{
    public State Submitted { get; private set; } = null!;
    public State AwaitingPayment { get; private set; } = null!;
    public State Completed { get; private set; } = null!;
    public State Cancelled { get; private set; } = null!;

    public Event<OrderPlacedIntegrationEvent> OrderPlaced { get; private set; } = null!;
    public Event<StockReservedEvent> StockReservedReceived { get; private set; } = null!;
    public Event<StockReservationFailedEvent> StockReservationFailedReceived { get; private set; } = null!;
    public Event<PaymentReservedIntegrationEvent> PaymentReservedReceived { get; private set; } = null!;
    public Event<PaymentFailedIntegrationEvent> PaymentFailedReceived { get; private set; } = null!;

    public OrderSaga(ILogger<OrderSaga> logger, BusinessMetrics metrics)
    {
        InstanceState(x => x.CurrentState);

        Event(() => OrderPlaced,
            x => x.CorrelateById(ctx => ctx.Message.OrderId));
        Event(() => StockReservedReceived,
            x => x.CorrelateById(ctx => ctx.Message.OrderId));
        Event(() => StockReservationFailedReceived,
            x => x.CorrelateById(ctx => ctx.Message.OrderId));
        Event(() => PaymentReservedReceived,
            x => x.CorrelateById(ctx => ctx.Message.OrderId));
        Event(() => PaymentFailedReceived,
            x => x.CorrelateById(ctx => ctx.Message.OrderId));

        Initially(
            When(OrderPlaced)
                .Then(ctx =>
                {
                    ctx.Saga.CustomerId = ctx.Message.CustomerId;
                    ctx.Saga.TotalAmount = ctx.Message.TotalAmount;
                    ctx.Saga.ItemsJson = JsonSerializer.Serialize(ctx.Message.Items);
                    metrics.OrderPlaced(ctx.Message.TotalAmount);
                    logger.LogInformation("Order saga started for order {OrderId}", ctx.Saga.CorrelationId);
                })
                .Publish(ctx => new ReserveStockCommand(
                    ctx.Saga.CorrelationId,
                    ctx.Message.Items
                        .Select(i => new StockCommandItem(i.ProductId, i.Quantity))
                        .ToList()))
                .TransitionTo(Submitted));

        During(Submitted,
            When(StockReservedReceived)
                .Then(ctx => logger.LogInformation(
                    "Stock reserved for order {OrderId}, requesting payment", ctx.Saga.CorrelationId))
                .Publish(ctx => new ProcessPaymentCommand(
                    ctx.Saga.CorrelationId,
                    ctx.Saga.CustomerId,
                    ctx.Saga.TotalAmount))
                .TransitionTo(AwaitingPayment),

            When(StockReservationFailedReceived)
                .Then(ctx =>
                {
                    ctx.Saga.FailureReason = ctx.Message.Reason;
                    metrics.OrderCancelled("stock_reservation_failed");
                })
                .Activity(x => x.OfInstanceType<CancelOrderActivity>())
                .Publish(ctx => new OrderCancelledIntegrationEvent(
                    ctx.Saga.CorrelationId,
                    ctx.Saga.CustomerId,
                    DateTimeOffset.UtcNow))
                .TransitionTo(Cancelled)
                .Finalize());

        During(AwaitingPayment,
            When(PaymentReservedReceived)
                .Then(ctx =>
                {
                    ctx.Saga.PaymentSucceeded = true;
                    ctx.Saga.PaymentId = ctx.Message.PaymentId;
                    ctx.Saga.TransactionId = ctx.Message.TransactionId;
                })
                .Activity(x => x.OfInstanceType<ConfirmOrderActivity>())
                .Publish(ctx => new OrderConfirmedIntegrationEvent(
                    ctx.Saga.CorrelationId,
                    ctx.Saga.CustomerId,
                    ctx.Saga.TotalAmount,
                    DateTimeOffset.UtcNow))
                .TransitionTo(Completed)
                .Finalize(),

            When(PaymentFailedReceived)
                .Then(ctx =>
                {
                    ctx.Saga.PaymentSucceeded = false;
                    ctx.Saga.FailureReason = ctx.Message.Reason;
                    metrics.OrderCancelled("payment_failed");
                })
                .Publish(ctx =>
                {
                    var items = JsonSerializer.Deserialize<List<OrderItemDto>>(ctx.Saga.ItemsJson)
                        ?? throw new InvalidOperationException(
                            $"Failed to deserialize ItemsJson for order saga {ctx.Saga.CorrelationId}.");
                    return new ReleaseStockCommand(
                        ctx.Saga.CorrelationId,
                        items.Select(i => new StockCommandItem(i.ProductId, i.Quantity)).ToList());
                })
                .Activity(x => x.OfInstanceType<CancelOrderActivity>())
                .Publish(ctx => new OrderCancelledIntegrationEvent(
                    ctx.Saga.CorrelationId,
                    ctx.Saga.CustomerId,
                    DateTimeOffset.UtcNow))
                .TransitionTo(Cancelled)
                .Finalize());

        SetCompletedWhenFinalized();
    }
}
