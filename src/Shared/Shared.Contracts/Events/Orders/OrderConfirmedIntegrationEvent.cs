using Shared.Contracts.Events;

namespace Shared.Contracts.Events.Orders;

public sealed record OrderConfirmedIntegrationEvent(
    Guid OrderId,
    Guid CustomerId,
    decimal TotalAmount,
    DateTimeOffset ConfirmedAt) : IntegrationEvent;
