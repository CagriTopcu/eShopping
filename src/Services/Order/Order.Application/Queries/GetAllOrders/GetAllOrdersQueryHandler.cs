using Mapster;
using Order.Application.Abstractions;
using Order.Application.DTOs;
using Shared.BuildingBlocks.CQRS;
using Shared.BuildingBlocks.Results;

namespace Order.Application.Queries.GetAllOrders;

internal sealed class GetAllOrdersQueryHandler(IOrderRepository orderRepository)
    : IQueryHandler<GetAllOrdersQuery, PagedOrderResponse>
{
    public async Task<Result<PagedOrderResponse>> Handle(
        GetAllOrdersQuery request,
        CancellationToken cancellationToken)
    {
        var (items, totalCount) = await orderRepository.GetAllPagedAsync(
            request.Page, request.PageSize, cancellationToken);

        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

        return new PagedOrderResponse(
            items.Adapt<List<OrderResponse>>(),
            request.Page,
            request.PageSize,
            totalCount,
            totalPages);
    }
}
