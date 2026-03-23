namespace Catalog.Application.DTOs;

/// <summary>Request payload for adjusting product stock.</summary>
/// <param name="Delta">Stock adjustment value. Positive to add, negative to remove.</param>
public sealed record AdjustStockRequest(int Delta);
