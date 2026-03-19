using Order.Application.DTOs;
using Shared.BuildingBlocks.CQRS;

namespace Order.Application.Commands.PlaceOrder;

public sealed record PlaceOrderCommand(
    Guid CustomerId,
    string Street,
    string City,
    string State,
    string Country,
    string ZipCode,
    IReadOnlyList<OrderItemRequest> Items) : ICommand<OrderResponse>;

public sealed record OrderItemRequest(
    Guid ProductId,
    string ProductName,
    decimal UnitPrice,
    int Quantity);
