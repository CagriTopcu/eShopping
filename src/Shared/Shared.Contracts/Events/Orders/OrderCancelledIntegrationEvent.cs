using Shared.Contracts.Events;

namespace Shared.Contracts.Events.Orders;

public sealed record OrderCancelledIntegrationEvent(
    Guid OrderId,
    Guid CustomerId,
    DateTimeOffset CancelledAt) : IntegrationEvent;
