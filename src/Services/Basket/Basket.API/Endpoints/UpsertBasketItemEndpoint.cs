using Basket.Application.Commands.UpsertBasketItem;
using Basket.Application.DTOs;
using MediatR;
using Shared.BuildingBlocks.Extensions;

namespace Basket.API.Endpoints;

internal static class UpsertBasketItemEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPut("/{username}/items", Handle)
            .WithName("UpsertBasketItem")
            .WithSummary("Add or update basket item")
            .WithDescription("Adds a product to the basket or updates its quantity if already present. Validates product existence and stock availability.")
            .Produces<BasketResponse>()
            .ProducesProblem(400)
            .ProducesProblem(404)
            .ProducesProblem(422)
            .RequireAuthorization("RequireCustomer");

    private static async Task<IResult> Handle(
        string username,
        UpsertBasketItemRequest request,
        ISender sender,
        CancellationToken ct) =>
        (await sender.Send(
            new UpsertBasketItemCommand(username, request.ProductId, request.Quantity), ct))
        .ToHttpResult();
}
