namespace Basket.Application.DTOs;

public sealed record ProductSnapshot(
    Guid ProductId,
    string Name,
    decimal Price,
    string Currency);
