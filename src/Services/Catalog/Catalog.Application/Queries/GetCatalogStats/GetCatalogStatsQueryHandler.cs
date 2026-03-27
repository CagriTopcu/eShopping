using Catalog.Application.Abstractions;
using Catalog.Application.DTOs;
using Shared.BuildingBlocks.CQRS;
using Shared.BuildingBlocks.Results;

namespace Catalog.Application.Queries.GetCatalogStats;

internal sealed class GetCatalogStatsQueryHandler(IProductReadRepository readRepository)
    : IQueryHandler<GetCatalogStatsQuery, CatalogStatsResponse>
{
    public async Task<Result<CatalogStatsResponse>> Handle(
        GetCatalogStatsQuery request,
        CancellationToken cancellationToken)
    {
        var stats = await readRepository.GetStatsAsync(cancellationToken);
        return stats;
    }
}
