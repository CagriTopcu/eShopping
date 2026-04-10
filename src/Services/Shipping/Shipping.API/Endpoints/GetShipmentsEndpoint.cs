using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shared.BuildingBlocks.Extensions;
using Shared.BuildingBlocks.Pagination;
using Shipping.Application.DTOs;
using Shipping.Application.Queries.GetShipments;
using Shipping.Domain.Enums;

namespace Shipping.API.Endpoints;

internal static class GetShipmentsEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("/", Handle)
            .WithName("GetShipments")
            .WithSummary("List shipments")
            .WithDescription("Returns a paginated list of shipments. Supports filtering by status. Requires Admin role.")
            .RequireAuthorization("RequireAdmin")
            .Produces<PagedList<ShipmentResponse>>();

    private static async Task<IResult> Handle(
        [FromQuery] ShipmentStatus? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        ISender sender = null!,
        CancellationToken ct = default)
    {
        var query = new GetShipmentsQuery(status, new PaginationParams(page, pageSize));
        return (await sender.Send(query, ct)).ToHttpResult();
    }
}
