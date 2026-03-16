namespace Stock.Application.DTOs;

public sealed record StockResponse(Guid ProductId, int AvailableQuantity);
