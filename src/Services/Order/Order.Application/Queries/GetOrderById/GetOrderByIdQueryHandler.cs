using Mapster;
using Order.Application.Abstractions;
using Order.Application.DTOs;
using Order.Domain.Errors;
using Shared.BuildingBlocks.CQRS;
using Shared.BuildingBlocks.Results;

namespace Order.Application.Queries.GetOrderById;

internal sealed class GetOrderByIdQueryHandler(IOrderRepository orderRepository)
    : IQueryHandler<GetOrderByIdQuery, OrderResponse>
{
    public async Task<Result<OrderResponse>> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetByIdAsync(request.OrderId, cancellationToken);

        if (order is null)
            return OrderErrors.NotFound;

        if (order.CustomerId != request.CustomerId)
            return Error.Forbidden("Order.Forbidden", "You do not have permission to view this order.");

        return order.Adapt<OrderResponse>();
    }
}
