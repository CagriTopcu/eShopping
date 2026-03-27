namespace Catalog.Application.DTOs;

public sealed record CatalogStatsResponse(
    int TotalProducts,
    int LowStockCount,
    IReadOnlyList<string> Categories);
