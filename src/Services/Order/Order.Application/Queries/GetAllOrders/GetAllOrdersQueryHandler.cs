using Mapster;
using Microsoft.Extensions.Logging;
using Order.Application.Abstractions;
using Order.Application.DTOs;
using Shared.BuildingBlocks.CQRS;
using Shared.BuildingBlocks.Results;

namespace Order.Application.Queries.GetAllOrders;

internal sealed class GetAllOrdersQueryHandler(
    IOrderRepository orderRepository,
    ILogger<GetAllOrdersQueryHandler> logger)
    : IQueryHandler<GetAllOrdersQuery, PagedOrderResponse>
{
    public async Task<Result<PagedOrderResponse>> Handle(
        GetAllOrdersQuery request,
        CancellationToken cancellationToken)
    {
        var (items, totalCount) = await orderRepository.GetAllPagedAsync(
            request.Page, request.PageSize, cancellationToken);

        logger.LogDebug(
            "Retrieved {PageSize} orders on page {Page} of {TotalPages} ({TotalCount} total)",
            items.Count, request.Page, (int)Math.Ceiling(totalCount / (double)request.PageSize), totalCount);

        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

        return new PagedOrderResponse(
            items.Adapt<List<OrderResponse>>(),
            request.Page,
            request.PageSize,
            totalCount,
            totalPages);
    }
}
