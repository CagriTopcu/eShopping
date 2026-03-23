using Order.Domain.Enums;

namespace Order.Application.DTOs;

/// <summary>Represents an order with its current state and details.</summary>
/// <param name="Id">Unique order identifier.</param>
/// <param name="CustomerId">Identifier of the customer who placed the order.</param>
/// <param name="Status">Current order status as enum value.</param>
/// <param name="StatusName">Human-readable order status name.</param>
/// <param name="Items">List of items in the order.</param>
/// <param name="TotalAmount">Sum of all item line totals.</param>
/// <param name="ShippingStreet">Shipping address street.</param>
/// <param name="ShippingCity">Shipping address city.</param>
/// <param name="ShippingState">Shipping address state or province.</param>
/// <param name="ShippingCountry">Shipping address country.</param>
/// <param name="ShippingZipCode">Shipping address postal/ZIP code.</param>
/// <param name="PlacedAt">Timestamp when the order was placed (UTC).</param>
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
