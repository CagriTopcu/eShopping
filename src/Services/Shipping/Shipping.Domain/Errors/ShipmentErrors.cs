using Shared.BuildingBlocks.Results;
using Shipping.Domain.Enums;

namespace Shipping.Domain.Errors;

public static class ShipmentErrors
{
    public static readonly Error NotFound =
        Error.NotFound("Shipment.NotFound", "Shipment was not found.");

    public static readonly Error AlreadyExists =
        Error.Conflict("Shipment.AlreadyExists", "A shipment already exists for this order.");

    public static readonly Error InvalidAction =
        Error.Validation("Shipment.InvalidAction", "The specified shipment action is not valid.");

    public static Error InvalidStatusTransition(ShipmentStatus from, ShipmentStatus to) =>
        Error.Validation("Shipment.InvalidStatusTransition",
            $"Cannot transition from {from} to {to}.");
}
