using Mapster;
using Microsoft.Extensions.Logging;
using Shared.BuildingBlocks.CQRS;
using Shared.BuildingBlocks.Pagination;
using Shared.BuildingBlocks.Results;
using Shipping.Application.Abstractions;
using Shipping.Application.DTOs;
using Shipping.Domain.Enums;

namespace Shipping.Application.Queries.GetShipments;

internal sealed class GetShipmentsQueryHandler(
    IShipmentRepository repository,
    ILogger<GetShipmentsQueryHandler> logger)
    : IQueryHandler<GetShipmentsQuery, PagedList<ShipmentResponse>>
{
    public async Task<Result<PagedList<ShipmentResponse>>> Handle(
        GetShipmentsQuery request,
        CancellationToken cancellationToken)
    {
        var (items, totalCount) = await repository.GetPagedAsync(
            request.Status,
            request.Pagination.Skip,
            request.Pagination.PageSize,
            cancellationToken);

        var responses = items.Select(s => s.Adapt<ShipmentResponse>()).ToList().AsReadOnly();
        var paged = PagedList<ShipmentResponse>.Create(
            responses,
            request.Pagination.Page,
            request.Pagination.PageSize,
            totalCount);

        logger.LogDebug(
            "GetShipments returned {Count} of {Total} shipments (page {Page})",
            responses.Count, totalCount, request.Pagination.Page);

        return paged;
    }
}
