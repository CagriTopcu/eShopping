using Shared.BuildingBlocks.CQRS;
using Shipping.Application.DTOs;

namespace Shipping.Application.Commands.UpdateShipmentStatus;

public sealed record UpdateShipmentStatusCommand(
    Guid ShipmentId,
    ShipmentAction Action,
    string? TrackingNumber) : ICommand<ShipmentResponse>;
