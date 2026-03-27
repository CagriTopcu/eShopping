using MediatR;
using Order.Application.Commands.CancelOrder;
using Shared.BuildingBlocks.Extensions;

namespace Order.API.Endpoints;

internal static class AdminCancelOrderEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapDelete("/admin/{id:guid}", Handle)
            .WithName("AdminCancelOrder")
            .WithSummary("Cancel any order (admin)")
            .WithDescription("Cancels any pending order regardless of ownership. Requires Admin role.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .RequireAuthorization("RequireAdmin");

    private static async Task<IResult> Handle(
        Guid id,
        ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new CancelOrderCommand(id, Guid.Empty, IsAdmin: true), ct);

        return result.IsSuccess ? Results.NoContent() : result.ToHttpResult();
    }
}
