using Shared.Contracts.Events;

namespace Shared.Contracts.Events.Orders;

public sealed record OrderPlacedIntegrationEvent(
    Guid OrderId,
    Guid CustomerId,
    IReadOnlyList<OrderItemDto> Items,
    decimal TotalAmount,
    DateTimeOffset PlacedAt) : IntegrationEvent;

public sealed record OrderItemDto(
    Guid ProductId,
    string ProductName,
    decimal UnitPrice,
    int Quantity);
