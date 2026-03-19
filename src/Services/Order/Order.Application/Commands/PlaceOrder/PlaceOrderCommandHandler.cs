using Mapster;
using MassTransit;
using Order.Application.Abstractions;
using Order.Application.DTOs;
using Order.Domain.ValueObjects;
using Shared.BuildingBlocks.CQRS;
using Shared.BuildingBlocks.Results;
using Shared.Contracts.Events.Orders;

namespace Order.Application.Commands.PlaceOrder;

internal sealed class PlaceOrderCommandHandler(
    IOrderRepository orderRepository,
    IPublishEndpoint publishEndpoint)
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

        await publishEndpoint.Publish(
            new OrderPlacedIntegrationEvent(
                result.Value.Id.Value,
                request.CustomerId,
                request.Items
                    .Select(i => new OrderItemDto(i.ProductId, i.ProductName, i.UnitPrice, i.Quantity))
                    .ToList(),
                result.Value.TotalAmount,
                DateTimeOffset.UtcNow),
            cancellationToken);

        await orderRepository.SaveChangesAsync(cancellationToken);

        return result.Value.Adapt<OrderResponse>();
    }
}
