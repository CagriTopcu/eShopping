using Shared.Contracts.Events;

namespace Shared.Contracts.Events.Payment;

public sealed record PaymentFailedIntegrationEvent(
    Guid OrderId,
    string Reason) : IntegrationEvent;
