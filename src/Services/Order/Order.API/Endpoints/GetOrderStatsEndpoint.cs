using MediatR;
using Order.Application.DTOs;
using Order.Application.Queries.GetOrderStats;
using Shared.BuildingBlocks.Extensions;

namespace Order.API.Endpoints;

internal static class GetOrderStatsEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("/stats", Handle)
            .WithName("GetOrderStats")
            .WithSummary("Get order statistics (admin)")
            .WithDescription("Returns aggregate order statistics. Requires Admin role.")
            .Produces<OrderStatsResponse>()
            .RequireAuthorization("RequireAdmin");

    private static async Task<IResult> Handle(
        ISender sender,
        CancellationToken ct) =>
        (await sender.Send(new GetOrderStatsQuery(), ct)).ToHttpResult();
}
