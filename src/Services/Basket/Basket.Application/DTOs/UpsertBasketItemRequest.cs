namespace Basket.Application.DTOs;

/// <summary>Request payload for adding or updating a basket item.</summary>
/// <param name="ProductId">Unique identifier of the product to add or update.</param>
/// <param name="Quantity">Desired quantity. Must be greater than zero.</param>
public sealed record UpsertBasketItemRequest(Guid ProductId, int Quantity);
