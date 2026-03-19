using MediatR;
using Order.Application.DTOs;
using Order.Application.Queries.GetOrderById;
using ServiceDefaults;
using Shared.BuildingBlocks.Extensions;

namespace Order.API.Endpoints;

internal static class GetOrderByIdEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("/{id:guid}", Handle)
            .WithName("GetOrderById")
            .Produces<OrderResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .RequireAuthorization("RequireCustomer");

    private static async Task<IResult> Handle(
        Guid id,
        ISender sender,
        ICurrentUser currentUser,
        CancellationToken ct) =>
        (await sender.Send(new GetOrderByIdQuery(id, currentUser.UserId!.Value), ct)).ToHttpResult();
}
