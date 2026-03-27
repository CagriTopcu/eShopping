using Order.Application.DTOs;
using Shared.BuildingBlocks.CQRS;

namespace Order.Application.Queries.GetAllOrders;

public sealed record GetAllOrdersQuery(int Page, int PageSize) : IQuery<PagedOrderResponse>;
