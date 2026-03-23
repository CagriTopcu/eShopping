using MediatR;
using Order.Application.DTOs;
using Order.Application.Queries.GetOrdersByUser;
using ServiceDefaults;
using Shared.BuildingBlocks.Extensions;

namespace Order.API.Endpoints;

internal static class GetOrdersByUserEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("/", Handle)
            .WithName("GetOrdersByUser")
            .WithSummary("List user orders")
            .WithDescription("Returns all orders for the authenticated user, ordered by most recent.")
            .Produces<IReadOnlyList<OrderResponse>>()
            .RequireAuthorization("RequireCustomer");

    private static async Task<IResult> Handle(
        ISender sender,
        ICurrentUser currentUser,
        CancellationToken ct) =>
        (await sender.Send(new GetOrdersByUserQuery(currentUser.UserId!.Value), ct)).ToHttpResult();
}
