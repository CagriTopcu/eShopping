using MassTransit;

namespace Order.Application.Sagas;

public sealed class OrderSagaState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; } = null!;
    public Guid CustomerId { get; set; }
    public decimal TotalAmount { get; set; }
    public string ItemsJson { get; set; } = "[]";
    public bool PaymentSucceeded { get; set; }
    public string? FailureReason { get; set; }
}
