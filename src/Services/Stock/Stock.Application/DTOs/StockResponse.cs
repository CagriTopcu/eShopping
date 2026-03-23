namespace Stock.Application.DTOs;

/// <summary>Represents the stock level for a product.</summary>
/// <param name="ProductId">Unique identifier of the product.</param>
/// <param name="AvailableQuantity">Current available stock quantity.</param>
public sealed record StockResponse(Guid ProductId, int AvailableQuantity);
