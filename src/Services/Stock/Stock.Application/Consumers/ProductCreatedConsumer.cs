using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Contracts.Events.Catalog;
using Stock.Application.Commands.SetStock;

namespace Stock.Application.Consumers;

public sealed class ProductCreatedConsumer(
    ISender sender,
    ILogger<ProductCreatedConsumer> logger) : IConsumer<ProductCreatedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<ProductCreatedIntegrationEvent> context)
    {
        var message = context.Message;

        logger.LogInformation(
            "Received ProductCreatedIntegrationEvent for product {ProductId} with stock {Stock}",
            message.ProductId, message.Stock);

        var result = await sender.Send(
            new SetStockCommand(message.ProductId, message.Stock),
            context.CancellationToken);

        if (result.IsFailure)
        {
            logger.LogWarning(
                "Failed to set initial stock for product {ProductId}: {Error}",
                message.ProductId, result.Error.Description);
            return;
        }

        logger.LogInformation(
            "Initial stock set for product {ProductId}: {Quantity}",
            message.ProductId, message.Stock);
    }
}
