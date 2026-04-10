using MassTransit;
using Microsoft.Extensions.Logging;
using Shared.BuildingBlocks.Observability;
using Shared.Contracts.Events.Orders;
using Shared.Contracts.Events.Shipping;
using Shipping.Application.Abstractions;
using Shipping.Domain.Entities;

namespace Shipping.Application.Consumers;

public sealed class OrderConfirmedConsumer(
    IShipmentRepository shipmentRepository,
    IPublishEndpoint publishEndpoint,
    ILogger<OrderConfirmedConsumer> logger) : IConsumer<OrderConfirmedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<OrderConfirmedIntegrationEvent> context)
    {
        using var activity = DiagnosticConfig.Source.StartActivity("shipment.create");
        var message = context.Message;

        activity?.SetTag("order.id", message.OrderId);

        var existing = await shipmentRepository.GetByOrderIdAsync(message.OrderId, context.CancellationToken);
        if (existing is not null)
        {
            logger.LogWarning("Shipment already exists for order {OrderId}. Skipping.", message.OrderId);
            return;
        }

        var shipment = Shipment.Create(message.OrderId, message.CustomerId, message.TotalAmount);

        await shipmentRepository.AddAsync(shipment, context.CancellationToken);
        await shipmentRepository.SaveChangesAsync(context.CancellationToken);

        logger.LogInformation("Shipment {ShipmentId} created for order {OrderId}",
            shipment.Id.Value, message.OrderId);

        await publishEndpoint.Publish(
            new ShipmentCreatedEvent(shipment.Id.Value, message.OrderId, message.CustomerId),
            context.CancellationToken);
    }
}
