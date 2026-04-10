using Shared.BuildingBlocks.CQRS;
using Shipping.Application.DTOs;

namespace Shipping.Application.Queries.GetShipment;

public sealed record GetShipmentQuery(Guid OrderId) : IQuery<ShipmentResponse>;
