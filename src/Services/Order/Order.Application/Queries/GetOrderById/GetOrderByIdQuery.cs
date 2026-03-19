using Order.Application.DTOs;
using Shared.BuildingBlocks.CQRS;

namespace Order.Application.Queries.GetOrderById;

public sealed record GetOrderByIdQuery(Guid OrderId, Guid CustomerId) : IQuery<OrderResponse>;
