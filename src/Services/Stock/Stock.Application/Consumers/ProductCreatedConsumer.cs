using MassTransit;
using Microsoft.Extensions.Logging;
using Shared.Contracts.Events.Catalog;
using Stock.Application.Abstractions;
using Stock.Domain.Entities;

namespace Stock.Application.Consumers;

/// <summary>
/// Initializes stock for newly created products. Idempotent: skips if stock already exists
/// to prevent overwriting quantities modified by subsequent reserve/release operations.
/// </summary>
public sealed class ProductCreatedConsumer(
    IStockRepository repository,
    ILogger<ProductCreatedConsumer> logger) : IConsumer<ProductCreatedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<ProductCreatedIntegrationEvent> context)
    {
        var message = context.Message;

        logger.LogInformation(
            "Received ProductCreatedIntegrationEvent for product {ProductId} with stock {Stock}",
            message.ProductId, message.Stock);

        var existing = await repository.GetByProductIdAsync(message.ProductId, context.CancellationToken);

        if (existing is not null)
        {
            logger.LogInformation(
                "Stock already exists for product {ProductId} (quantity: {Quantity}). Skipping duplicate event",
                message.ProductId, existing.AvailableQuantity);
            return;
        }

        var item = StockItem.Create(message.ProductId, message.Stock);
        await repository.AddAsync(item, context.CancellationToken);
        await repository.SaveChangesAsync(context.CancellationToken);

        logger.LogInformation(
            "Initial stock created for product {ProductId}: {Quantity}",
            message.ProductId, message.Stock);
    }
}
