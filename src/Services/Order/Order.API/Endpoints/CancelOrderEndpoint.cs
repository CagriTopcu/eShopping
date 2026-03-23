using MediatR;
using Order.Application.Commands.CancelOrder;
using ServiceDefaults;
using Shared.BuildingBlocks.Extensions;

namespace Order.API.Endpoints;

internal static class CancelOrderEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapDelete("/{id:guid}", Handle)
            .WithName("CancelOrder")
            .WithSummary("Cancel an order")
            .WithDescription("Cancels a pending order. Only orders in Pending or PaymentReserved status can be cancelled (409 Conflict otherwise).")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .RequireAuthorization("RequireCustomer");

    private static async Task<IResult> Handle(
        Guid id,
        ISender sender,
        ICurrentUser currentUser,
        CancellationToken ct)
    {
        var result = await sender.Send(new CancelOrderCommand(id, currentUser.UserId!.Value), ct);

        return result.IsSuccess ? Results.NoContent() : result.ToHttpResult();
    }
}
