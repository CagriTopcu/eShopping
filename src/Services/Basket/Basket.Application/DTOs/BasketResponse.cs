namespace Basket.Application.DTOs;

/// <summary>Represents a user's shopping basket.</summary>
/// <param name="Username">Username of the basket owner.</param>
/// <param name="Items">List of items in the basket.</param>
/// <param name="TotalPrice">Sum of all item line totals.</param>
public sealed record BasketResponse(
    string Username,
    IReadOnlyList<BasketItemResponse> Items,
    decimal TotalPrice);
