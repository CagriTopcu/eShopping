using Catalog.Application.DTOs;
using Catalog.Application.ReadModels;
using Shared.BuildingBlocks.Pagination;

namespace Catalog.Application.Abstractions;

public interface IProductReadRepository
{
    Task<ProductReadModel?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<(IReadOnlyList<ProductReadModel> Items, int TotalCount)> GetPagedAsync(
        string? category,
        string? name,
        decimal? minPrice,
        decimal? maxPrice,
        PaginationParams pagination,
        CancellationToken ct = default);

    Task<CatalogStatsResponse> GetStatsAsync(CancellationToken ct = default);

    Task UpsertAsync(ProductReadModel model, CancellationToken ct = default);
    Task MarkDeletedAsync(Guid id, CancellationToken ct = default);
}
