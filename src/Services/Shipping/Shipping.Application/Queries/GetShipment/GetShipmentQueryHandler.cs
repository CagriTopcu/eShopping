using Mapster;
using Microsoft.Extensions.Logging;
using Shared.BuildingBlocks.CQRS;
using Shared.BuildingBlocks.Results;
using Shipping.Application.Abstractions;
using Shipping.Application.DTOs;
using Shipping.Domain.Errors;

namespace Shipping.Application.Queries.GetShipment;

internal sealed class GetShipmentQueryHandler(
    IShipmentRepository repository,
    ILogger<GetShipmentQueryHandler> logger)
    : IQueryHandler<GetShipmentQuery, ShipmentResponse>
{
    public async Task<Result<ShipmentResponse>> Handle(GetShipmentQuery request, CancellationToken cancellationToken)
    {
        var shipment = await repository.GetByOrderIdAsync(request.OrderId, cancellationToken);

        if (shipment is null)
        {
            logger.LogDebug("Shipment not found for order {OrderId}", request.OrderId);
            return ShipmentErrors.NotFound;
        }

        return shipment.Adapt<ShipmentResponse>();
    }
}
