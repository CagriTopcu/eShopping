using Order.Application.DTOs;
using Shared.BuildingBlocks.CQRS;

namespace Order.Application.Queries.GetOrderStats;

public sealed record GetOrderStatsQuery : IQuery<OrderStatsResponse>;
