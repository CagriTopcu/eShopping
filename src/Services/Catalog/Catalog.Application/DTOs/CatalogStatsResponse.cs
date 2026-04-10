namespace Catalog.Application.DTOs;

public sealed record CatalogStatsResponse(
    int TotalProducts,
    IReadOnlyList<string> Categories);
