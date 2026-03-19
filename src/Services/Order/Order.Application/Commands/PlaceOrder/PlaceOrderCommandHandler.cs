using Mapster;
using Order.Application.Abstractions;
using Order.Application.DTOs;
using Order.Domain.Entities;
using Order.Domain.ValueObjects;
using Shared.BuildingBlocks.CQRS;
using Shared.BuildingBlocks.Results;

namespace Order.Application.Commands.PlaceOrder;

internal sealed class PlaceOrderCommandHandler(IOrderRepository orderRepository)
    : ICommandHandler<PlaceOrderCommand, OrderResponse>
{
    public async Task<Result<OrderResponse>> Handle(PlaceOrderCommand request, CancellationToken cancellationToken)
    {
        var address = new Address(
            request.Street,
            request.City,
            request.State,
            request.Country,
            request.ZipCode);

        var items = request.Items.Select(i =>
            (i.ProductId, i.ProductName, i.UnitPrice, i.Quantity));

        var result = Order.Domain.Entities.Order.Place(request.CustomerId, address, items);

        if (result.IsFailure)
            return result.Error;

        await orderRepository.AddAsync(result.Value, cancellationToken);
        await orderRepository.SaveChangesAsync(cancellationToken);

        return result.Value.Adapt<OrderResponse>();
    }
}
