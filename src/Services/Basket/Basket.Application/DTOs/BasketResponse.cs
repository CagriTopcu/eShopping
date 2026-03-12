namespace Basket.Application.DTOs;

public sealed record BasketResponse(
    string Username,
    IReadOnlyList<BasketItemResponse> Items,
    decimal TotalPrice);
