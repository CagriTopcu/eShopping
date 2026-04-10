using Catalog.Application.Abstractions;
using Catalog.Application.DTOs;
using Microsoft.Extensions.Logging;
using Shared.BuildingBlocks.CQRS;
using Shared.BuildingBlocks.Results;

namespace Catalog.Application.Queries.GetCatalogStats;

internal sealed class GetCatalogStatsQueryHandler(
    IProductReadRepository readRepository,
    ILogger<GetCatalogStatsQueryHandler> logger)
    : IQueryHandler<GetCatalogStatsQuery, CatalogStatsResponse>
{
    public async Task<Result<CatalogStatsResponse>> Handle(
        GetCatalogStatsQuery request,
        CancellationToken cancellationToken)
    {
        var stats = await readRepository.GetStatsAsync(cancellationToken);

        logger.LogDebug(
            "CatalogStats: {TotalProducts} total products, {CategoryCount} categories",
            stats.TotalProducts, stats.Categories.Count);

        return stats;
    }
}
