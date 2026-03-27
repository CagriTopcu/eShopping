using Catalog.Application.DTOs;
using Catalog.Application.Queries.GetCatalogStats;
using MediatR;
using Shared.BuildingBlocks.Extensions;

namespace Catalog.API.Endpoints;

internal static class GetCatalogStatsEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("/stats", Handle)
            .WithName("GetCatalogStats")
            .WithSummary("Get catalog statistics (admin)")
            .WithDescription("Returns aggregate catalog statistics including total products, low stock count, and categories. Requires Admin role.")
            .Produces<CatalogStatsResponse>()
            .RequireAuthorization("RequireAdmin");

    private static async Task<IResult> Handle(
        ISender sender,
        CancellationToken ct) =>
        (await sender.Send(new GetCatalogStatsQuery(), ct)).ToHttpResult();
}
