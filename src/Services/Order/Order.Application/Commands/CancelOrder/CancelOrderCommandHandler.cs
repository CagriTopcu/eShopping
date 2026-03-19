using Order.Application.Abstractions;
using Order.Domain.Errors;
using Shared.BuildingBlocks.CQRS;
using Shared.BuildingBlocks.Results;

namespace Order.Application.Commands.CancelOrder;

internal sealed class CancelOrderCommandHandler(IOrderRepository orderRepository)
    : ICommandHandler<CancelOrderCommand>
{
    public async Task<Result> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetByIdAsync(request.OrderId, cancellationToken);

        if (order is null)
            return OrderErrors.NotFound;

        if (order.CustomerId != request.CustomerId)
            return Error.Forbidden("Order.Forbidden", "You do not have permission to cancel this order.");

        var result = order.Cancel();

        if (result.IsFailure)
            return result;

        await orderRepository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
