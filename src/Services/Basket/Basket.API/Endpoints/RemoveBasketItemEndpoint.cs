using Basket.Application.Commands.RemoveBasketItem;
using MediatR;
using Shared.BuildingBlocks.Extensions;

namespace Basket.API.Endpoints;

internal static class RemoveBasketItemEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapDelete("/{username}/items/{productId:guid}", Handle)
            .WithName("RemoveBasketItem")
            .WithSummary("Remove item from basket")
            .WithDescription("Removes a specific product from the user's basket.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(404)
            .RequireAuthorization("RequireCustomer");

    private static async Task<IResult> Handle(
        string username,
        Guid productId,
        ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new RemoveBasketItemCommand(username, productId), ct);

        if (result.IsFailure)
            return result.ToHttpResult();

        return Results.NoContent();
    }
}
