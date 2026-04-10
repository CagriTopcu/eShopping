using MediatR;
using Shared.BuildingBlocks.Extensions;
using Shipping.Application.DTOs;
using Shipping.Application.Queries.GetShipment;

namespace Shipping.API.Endpoints;

internal static class GetShipmentEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("/{orderId:guid}", Handle)
            .WithName("GetShipment")
            .WithSummary("Get shipment by order ID")
            .WithDescription("Returns the shipment details for a specific order.")
            .Produces<ShipmentResponse>()
            .ProducesProblem(404);

    private static async Task<IResult> Handle(
        Guid orderId,
        ISender sender,
        CancellationToken ct) =>
        (await sender.Send(new GetShipmentQuery(orderId), ct)).ToHttpResult();
}
