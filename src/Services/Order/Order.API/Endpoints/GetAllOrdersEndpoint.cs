using MediatR;
using Order.Application.DTOs;
using Order.Application.Queries.GetAllOrders;
using Shared.BuildingBlocks.Extensions;

namespace Order.API.Endpoints;

internal static class GetAllOrdersEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("/admin", Handle)
            .WithName("GetAllOrders")
            .WithSummary("List all orders (admin)")
            .WithDescription("Returns all orders with pagination. Requires Admin role.")
            .Produces<PagedOrderResponse>()
            .RequireAuthorization("RequireAdmin");

    private static async Task<IResult> Handle(
        int page,
        int pageSize,
        ISender sender,
        CancellationToken ct) =>
        (await sender.Send(new GetAllOrdersQuery(page, pageSize), ct)).ToHttpResult();
}
