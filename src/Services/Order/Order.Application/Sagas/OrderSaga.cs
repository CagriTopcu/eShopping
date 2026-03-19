using MassTransit;
using Microsoft.Extensions.Logging;
using Order.Application.Sagas.Activities;
using Shared.Contracts.Commands.Stock;
using Shared.Contracts.Events.Orders;
using Shared.Contracts.Events.Stock;
using System.Text.Json;

namespace Order.Application.Sagas;

public sealed class OrderSaga : MassTransitStateMachine<OrderSagaState>
{
    public State Submitted { get; private set; } = null!;
    public State Completed { get; private set; } = null!;
    public State Cancelled { get; private set; } = null!;

    public Event<OrderPlacedIntegrationEvent> OrderPlaced { get; private set; } = null!;
    public Event<StockReservedEvent> StockReservedReceived { get; private set; } = null!;
    public Event<StockReservationFailedEvent> StockReservationFailedReceived { get; private set; } = null!;

    public OrderSaga(ILogger<OrderSaga> logger)
    {
        InstanceState(x => x.CurrentState);

        Event(() => OrderPlaced,
            x => x.CorrelateById(ctx => ctx.Message.OrderId));
        Event(() => StockReservedReceived,
            x => x.CorrelateById(ctx => ctx.Message.OrderId));
        Event(() => StockReservationFailedReceived,
            x => x.CorrelateById(ctx => ctx.Message.OrderId));

        Initially(
            When(OrderPlaced)
                .Then(ctx =>
                {
                    ctx.Saga.CustomerId = ctx.Message.CustomerId;
                    ctx.Saga.TotalAmount = ctx.Message.TotalAmount;
                    ctx.Saga.ItemsJson = JsonSerializer.Serialize(ctx.Message.Items);
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
                .Activity(x => x.OfInstanceType<ProcessPaymentActivity>())
                .IfElse(
                    ctx => ctx.Saga.PaymentSucceeded,
                    success => success.TransitionTo(Completed).Finalize(),
                    fail => fail.TransitionTo(Cancelled).Finalize()),

            When(StockReservationFailedReceived)
                .Then(ctx => ctx.Saga.FailureReason = ctx.Message.Reason)
                .Activity(x => x.OfInstanceType<CancelOrderActivity>())
                .TransitionTo(Cancelled)
                .Finalize());

        SetCompletedWhenFinalized();
    }
}
