namespace Order.Application.DTOs;

/// <summary>Represents a single item within an order.</summary>
/// <param name="ProductId">Unique identifier of the ordered product.</param>
/// <param name="ProductName">Display name of the product at time of order.</param>
/// <param name="UnitPrice">Price per unit at time of order.</param>
/// <param name="Quantity">Number of units ordered.</param>
/// <param name="LineTotal">Total price for this item (UnitPrice * Quantity).</param>
public sealed record OrderItemResponse(
    Guid ProductId,
    string ProductName,
    decimal UnitPrice,
    int Quantity,
    decimal LineTotal);
