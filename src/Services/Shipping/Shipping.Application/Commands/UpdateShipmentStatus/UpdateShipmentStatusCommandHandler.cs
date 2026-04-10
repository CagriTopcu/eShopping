using Mapster;
using Microsoft.Extensions.Logging;
using Shared.BuildingBlocks.CQRS;
using Shared.BuildingBlocks.Results;
using Shipping.Application.Abstractions;
using Shipping.Application.DTOs;
using Shipping.Domain.Errors;

namespace Shipping.Application.Commands.UpdateShipmentStatus;

internal sealed class UpdateShipmentStatusCommandHandler(
    IShipmentRepository repository,
    ILogger<UpdateShipmentStatusCommandHandler> logger)
    : ICommandHandler<UpdateShipmentStatusCommand, ShipmentResponse>
{
    public async Task<Result<ShipmentResponse>> Handle(
        UpdateShipmentStatusCommand request,
        CancellationToken cancellationToken)
    {
        var shipment = await repository.GetByIdAsync(request.ShipmentId, cancellationToken);

        if (shipment is null)
            return ShipmentErrors.NotFound;

        var result = request.Action switch
        {
            ShipmentAction.Ship => shipment.MarkShipped(request.TrackingNumber!),
            ShipmentAction.Deliver => shipment.MarkDelivered(),
            ShipmentAction.Fail => shipment.MarkFailed(),
            _ => Result.Failure(ShipmentErrors.InvalidAction)
        };

        if (result.IsFailure)
            return result.Error;

        await repository.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Shipment {ShipmentId} status updated to {Action}",
            request.ShipmentId, request.Action);

        return shipment.Adapt<ShipmentResponse>();
    }
}
