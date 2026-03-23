namespace Basket.Application.DTOs;

/// <summary>Represents a single item in the shopping basket.</summary>
/// <param name="ProductId">Unique identifier of the product.</param>
/// <param name="ProductName">Display name of the product.</param>
/// <param name="UnitPrice">Price per unit at time of addition.</param>
/// <param name="Currency">ISO 4217 currency code.</param>
/// <param name="Quantity">Number of units in the basket.</param>
/// <param name="LineTotal">Total price for this item (UnitPrice * Quantity).</param>
public sealed record BasketItemResponse(
    Guid ProductId,
    string ProductName,
    decimal UnitPrice,
    string Currency,
    int Quantity,
    decimal LineTotal);
