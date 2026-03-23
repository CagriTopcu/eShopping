using Basket.Application.Commands.DeleteBasket;
using MediatR;
using Shared.BuildingBlocks.Extensions;

namespace Basket.API.Endpoints;

internal static class DeleteBasketEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapDelete("/{username}", Handle)
            .WithName("DeleteBasket")
            .WithSummary("Clear basket")
            .WithDescription("Removes all items from the user's basket.")
            .Produces(StatusCodes.Status204NoContent)
            .RequireAuthorization("RequireCustomer");

    private static async Task<IResult> Handle(
        string username,
        ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new DeleteBasketCommand(username), ct);

        if (result.IsFailure)
            return result.ToHttpResult();

        return Results.NoContent();
    }
}
