using Order.Domain.Enums;

namespace Order.Application.DTOs;

public sealed record OrderResponse(
    Guid Id,
    Guid CustomerId,
    OrderStatus Status,
    string StatusName,
    IReadOnlyList<OrderItemResponse> Items,
    decimal TotalAmount,
    string ShippingStreet,
    string ShippingCity,
    string ShippingState,
    string ShippingCountry,
    string ShippingZipCode,
    DateTime PlacedAt);
