using Asp.Versioning;
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
            .WithSummary("Place a new order")
            .WithDescription("Creates an order from the provided items. Triggers payment reservation and stock reservation via saga orchestration.")
            .Produces<OrderResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
            .RequireAuthorization("RequireCustomer");

    private static async Task<IResult> Handle(
        PlaceOrderRequest request,
        ISender sender,
        ICurrentUser currentUser,
        HttpContext context,
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

        var version = context.GetRequestedApiVersion();
        return result.IsSuccess
            ? Results.Created($"/api/v{version}/orders/{result.Value.Id}", result.Value)
            : result.ToHttpResult();
    }
}

/// <summary>Request payload for placing a new order.</summary>
/// <param name="Street">Shipping address street.</param>
/// <param name="City">Shipping address city.</param>
/// <param name="State">Shipping address state or province.</param>
/// <param name="Country">Shipping address country.</param>
/// <param name="ZipCode">Shipping address postal/ZIP code.</param>
/// <param name="Items">List of items to order with product IDs and quantities.</param>
internal sealed record PlaceOrderRequest(
    string Street,
    string City,
    string State,
    string Country,
    string ZipCode,
    IReadOnlyList<OrderItemRequest> Items);
