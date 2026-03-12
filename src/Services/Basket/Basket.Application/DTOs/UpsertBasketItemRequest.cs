namespace Basket.Application.DTOs;

public sealed record UpsertBasketItemRequest(Guid ProductId, int Quantity);
