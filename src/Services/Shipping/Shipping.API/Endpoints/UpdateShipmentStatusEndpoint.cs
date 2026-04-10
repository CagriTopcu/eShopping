using MediatR;
using Shared.BuildingBlocks.Extensions;
using Shipping.Application.Commands.UpdateShipmentStatus;
using Shipping.Application.DTOs;

namespace Shipping.API.Endpoints;

internal static class UpdateShipmentStatusEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPatch("/{shipmentId:guid}/status", Handle)
            .WithName("UpdateShipmentStatus")
            .WithSummary("Update shipment status")
            .WithDescription("Updates the status of a shipment (Ship, Deliver, or Fail). Requires Admin role.")
            .RequireAuthorization("RequireAdmin")
            .Produces<ShipmentResponse>()
            .ProducesProblem(404)
            .ProducesValidationProblem();

    private static async Task<IResult> Handle(
        Guid shipmentId,
        UpdateShipmentStatusRequest request,
        ISender sender,
        CancellationToken ct) =>
        (await sender.Send(new UpdateShipmentStatusCommand(shipmentId, request.Action, request.TrackingNumber), ct))
            .ToHttpResult();
}

internal sealed record UpdateShipmentStatusRequest(ShipmentAction Action, string? TrackingNumber);
