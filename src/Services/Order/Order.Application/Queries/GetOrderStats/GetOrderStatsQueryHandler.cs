using Microsoft.Extensions.Logging;
using Order.Application.Abstractions;
using Order.Application.DTOs;
using Order.Domain.Enums;
using Shared.BuildingBlocks.CQRS;
using Shared.BuildingBlocks.Results;

namespace Order.Application.Queries.GetOrderStats;

internal sealed class GetOrderStatsQueryHandler(
    IOrderRepository orderRepository,
    ILogger<GetOrderStatsQueryHandler> logger)
    : IQueryHandler<GetOrderStatsQuery, OrderStatsResponse>
{
    public async Task<Result<OrderStatsResponse>> Handle(
        GetOrderStatsQuery request,
        CancellationToken cancellationToken)
    {
        var stats = await orderRepository.GetStatsAsync(cancellationToken);

        logger.LogDebug(
            "Order stats retrieved: {TotalOrders} total orders, {TotalRevenue} total revenue",
            stats.TotalOrders, stats.TotalRevenue);

        return stats;
    }
}
