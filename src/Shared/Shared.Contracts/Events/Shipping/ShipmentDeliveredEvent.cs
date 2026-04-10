using Shared.Contracts.Events;

namespace Shared.Contracts.Events.Shipping;

public sealed record ShipmentDeliveredEvent(
    Guid ShipmentId,
    Guid OrderId,
    Guid CustomerId) : IntegrationEvent;
