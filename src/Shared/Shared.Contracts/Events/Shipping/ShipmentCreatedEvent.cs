using Shared.Contracts.Events;

namespace Shared.Contracts.Events.Shipping;

public sealed record ShipmentCreatedEvent(
    Guid ShipmentId,
    Guid OrderId,
    Guid CustomerId) : IntegrationEvent;
