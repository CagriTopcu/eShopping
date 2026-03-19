using MediatR;
using Order.Application.Commands.PlaceOrder;
using Order.Application.DTOs;
using ServiceDefaults;
using Shared.BuildingBlocks.Extensions;

namespace Order.API.Endpoints;

internal static class PlaceOrderEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("/", Handle)
            .WithName("PlaceOrder")
            .Produces<OrderResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
            .RequireAuthorization("RequireCustomer");

    private static async Task<IResult> Handle(
        PlaceOrderRequest request,
        ISender sender,
        ICurrentUser currentUser,
        CancellationToken ct)
    {
        var command = new PlaceOrderCommand(
            currentUser.UserId!.Value,
            request.Street,
            request.City,
            request.State,
            request.Country,
            request.ZipCode,
            request.Items);

        var result = await sender.Send(command, ct);

        return result.IsSuccess
            ? Results.Created($"/api/v1/orders/{result.Value.Id}", result.Value)
            : result.ToHttpResult();
    }
}

internal sealed record PlaceOrderRequest(
    string Street,
    string City,
    string State,
    string Country,
    string ZipCode,
    IReadOnlyList<OrderItemRequest> Items);
