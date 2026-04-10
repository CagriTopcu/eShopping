using Shared.Contracts.Events;

namespace Shared.Contracts.Events.Payment;

public sealed record PaymentReservedIntegrationEvent(
    Guid OrderId,
    Guid PaymentId,
    string TransactionId) : IntegrationEvent;
