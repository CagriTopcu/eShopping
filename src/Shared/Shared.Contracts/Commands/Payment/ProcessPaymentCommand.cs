namespace Shared.Contracts.Commands.Payment;

public sealed record ProcessPaymentCommand(
    Guid OrderId,
    Guid CustomerId,
    decimal Amount,
    string Currency = "USD");
