using MassTransit;
using Microsoft.Extensions.Logging;
using Order.Application.Abstractions;
using Shared.Contracts.Commands.Stock;
using Shared.Contracts.Events.Orders;
using System.Text.Json;

namespace Order.Application.Sagas.Activities;

public sealed class ProcessPaymentActivity(
    IPaymentClient paymentClient,
    IOrderRepository orderRepository,
    ILogger<ProcessPaymentActivity> logger)
    : IStateMachineActivity<OrderSagaState>
{
    public async Task Execute(
        BehaviorContext<OrderSagaState> context,
        IBehavior<OrderSagaState> next)
    {
        var saga = context.Saga;

        var paymentSuccess = await paymentClient.ReserveAsync(
            saga.CorrelationId,
            saga.CustomerId,
            saga.TotalAmount,
            context.CancellationToken);

        var order = await orderRepository.GetByIdAsync(saga.CorrelationId, context.CancellationToken);

        if (order is not null)
        {
            if (paymentSuccess)
            {
                order.Confirm();
                logger.LogInformation("Order {OrderId} confirmed after payment", saga.CorrelationId);
            }
            else
            {
                order.Cancel();
                logger.LogWarning("Order {OrderId} cancelled — payment failed", saga.CorrelationId);

                var items = JsonSerializer.Deserialize<List<OrderItemDto>>(saga.ItemsJson)!;
                await context.Publish(new ReleaseStockCommand(
                    saga.CorrelationId,
                    items.Select(i => new StockCommandItem(i.ProductId, i.Quantity)).ToList()),
                    context.CancellationToken);
            }

            await orderRepository.SaveChangesAsync(context.CancellationToken);
        }

        saga.PaymentSucceeded = paymentSuccess;

        await next.Execute(context);
    }

    public Task Execute<T>(BehaviorContext<OrderSagaState, T> context, IBehavior<OrderSagaState, T> next)
        where T : class => next.Execute(context);

    public Task Faulted<TException>(
        BehaviorExceptionContext<OrderSagaState, TException> context,
        IBehavior<OrderSagaState> next)
        where TException : Exception => next.Faulted(context);

    public Task Faulted<T, TException>(
        BehaviorExceptionContext<OrderSagaState, T, TException> context,
        IBehavior<OrderSagaState, T> next)
        where T : class where TException : Exception => next.Faulted(context);

    public void Probe(ProbeContext context) { }

    public void Accept(StateMachineVisitor visitor) => visitor.Visit(this);
}
