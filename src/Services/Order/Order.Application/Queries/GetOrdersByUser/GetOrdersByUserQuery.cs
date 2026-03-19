using Order.Application.DTOs;
using Shared.BuildingBlocks.CQRS;

namespace Order.Application.Queries.GetOrdersByUser;

public sealed record GetOrdersByUserQuery(Guid CustomerId) : IQuery<IReadOnlyList<OrderResponse>>;
