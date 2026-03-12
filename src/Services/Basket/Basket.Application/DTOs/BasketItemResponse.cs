namespace Basket.Application.DTOs;

public sealed record BasketItemResponse(
    Guid ProductId,
    string ProductName,
    decimal UnitPrice,
    string Currency,
    int Quantity,
    decimal LineTotal);
