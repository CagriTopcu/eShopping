using Catalog.Application.Abstractions;
using Catalog.Application.DTOs;
using Mapster;
using Microsoft.Extensions.Logging;
using Shared.BuildingBlocks.CQRS;
using Shared.BuildingBlocks.Pagination;
using Shared.BuildingBlocks.Results;

namespace Catalog.Application.Queries.GetProducts;

internal sealed class GetProductsQueryHandler(
    IProductReadRepository readRepository,
    ILogger<GetProductsQueryHandler> logger)
    : IQueryHandler<GetProductsQuery, PagedList<ProductResponse>>
{
    public async Task<Result<PagedList<ProductResponse>>> Handle(
        GetProductsQuery request,
        CancellationToken cancellationToken)
    {
        var (items, totalCount) = await readRepository.GetPagedAsync(
            request.Category,
            request.Name,
            request.MinPrice,
            request.MaxPrice,
            request.Pagination,
            cancellationToken);

        var responses = items.Select(m => m.Adapt<ProductResponse>()).ToList().AsReadOnly();
        var paged = PagedList<ProductResponse>.Create(
            responses,
            request.Pagination.Page,
            request.Pagination.PageSize,
            totalCount);

        logger.LogDebug(
            "GetProducts returned {Count} of {Total} products (page {Page}/{PageSize})",
            responses.Count, totalCount, request.Pagination.Page, request.Pagination.PageSize);

        return paged;
    }
}
