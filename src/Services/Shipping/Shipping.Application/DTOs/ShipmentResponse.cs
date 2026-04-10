using Shipping.Domain.Enums;

namespace Shipping.Application.DTOs;

public sealed record ShipmentResponse(
    Guid Id,
    Guid OrderId,
    Guid CustomerId,
    decimal OrderTotal,
    ShipmentStatus Status,
    string? TrackingNumber,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ShippedAt,
    DateTimeOffset? DeliveredAt);
