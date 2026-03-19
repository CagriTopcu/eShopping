using MassTransit;
using Microsoft.Extensions.Logging;
using Order.Application.Abstractions;

namespace Order.Application.Sagas.Activities;

public sealed class CancelOrderActivity(
    IOrderRepository orderRepository,
    ILogger<CancelOrderActivity> logger)
    : IStateMachineActivity<OrderSagaState>
{
    public async Task Execute(
        BehaviorContext<OrderSagaState> context,
        IBehavior<OrderSagaState> next)
    {
        var order = await orderRepository.GetByIdAsync(
            context.Saga.CorrelationId,
            context.CancellationToken);

        if (order is not null)
        {
            order.Cancel();
            await orderRepository.SaveChangesAsync(context.CancellationToken);
        }

        logger.LogWarning("Order {OrderId} cancelled — {Reason}",
            context.Saga.CorrelationId, context.Saga.FailureReason ?? "stock reservation failed");

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
