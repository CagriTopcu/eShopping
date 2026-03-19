using Mapster;
using Order.Application.Abstractions;
using Order.Application.DTOs;
using Shared.BuildingBlocks.CQRS;
using Shared.BuildingBlocks.Results;

namespace Order.Application.Queries.GetOrdersByUser;

internal sealed class GetOrdersByUserQueryHandler(IOrderRepository orderRepository)
    : IQueryHandler<GetOrdersByUserQuery, IReadOnlyList<OrderResponse>>
{
    public async Task<Result<IReadOnlyList<OrderResponse>>> Handle(
        GetOrdersByUserQuery request,
        CancellationToken cancellationToken)
    {
        var orders = await orderRepository.GetByCustomerIdAsync(request.CustomerId, cancellationToken);

        return Result.Success<IReadOnlyList<OrderResponse>>(
            orders.Adapt<List<OrderResponse>>());
    }
}
