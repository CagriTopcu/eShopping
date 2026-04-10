using Basket.Application.Abstractions;
using Basket.Application.DTOs;
using MassTransit;
using Microsoft.Extensions.Logging;
using Shared.Contracts.Events.Catalog;

namespace Basket.Infrastructure.Consumers;

/// <summary>
/// Maintains a Redis-backed product snapshot cache by consuming Catalog integration events.
/// This eliminates the need for sync HTTP calls to Catalog during add-to-cart.
/// </summary>
public sealed class ProductCreatedSnapshotConsumer(
    IProductSnapshotCache cache,
    ILogger<ProductCreatedSnapshotConsumer> logger) : IConsumer<ProductCreatedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<ProductCreatedIntegrationEvent> context)
    {
        var msg = context.Message;

        logger.LogInformation(
            "Caching product snapshot for new product {ProductId} ({Name})",
            msg.ProductId, msg.Name);

        await cache.SetAsync(
            new ProductSnapshot(msg.ProductId, msg.Name, msg.Price, msg.Currency),
            context.CancellationToken);
    }
}

public sealed class ProductUpdatedSnapshotConsumer(
    IProductSnapshotCache cache,
    ILogger<ProductUpdatedSnapshotConsumer> logger) : IConsumer<ProductUpdatedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<ProductUpdatedIntegrationEvent> context)
    {
        var msg = context.Message;

        logger.LogInformation(
            "Updating product snapshot for {ProductId} ({Name})",
            msg.ProductId, msg.Name);

        await cache.SetAsync(
            new ProductSnapshot(msg.ProductId, msg.Name, msg.Price, msg.Currency),
            context.CancellationToken);
    }
}

public sealed class ProductPriceChangedSnapshotConsumer(
    IProductSnapshotCache cache,
    ILogger<ProductPriceChangedSnapshotConsumer> logger) : IConsumer<ProductPriceChangedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<ProductPriceChangedIntegrationEvent> context)
    {
        var msg = context.Message;

        logger.LogInformation(
            "Updating cached price for product {ProductId}: {OldPrice} -> {NewPrice} {Currency}",
            msg.ProductId, msg.OldPrice, msg.NewPrice, msg.Currency);

        // Read existing snapshot to preserve name, then update price.
        var existing = await cache.GetAsync(msg.ProductId, context.CancellationToken);
        if (existing is null)
        {
            logger.LogWarning(
                "No cached snapshot for product {ProductId} — price change event ignored (will be populated on next ProductCreated/Updated)",
                msg.ProductId);
            return;
        }

        await cache.SetAsync(
            existing with { Price = msg.NewPrice, Currency = msg.Currency },
            context.CancellationToken);
    }
}

public sealed class ProductDeletedSnapshotConsumer(
    IProductSnapshotCache cache,
    ILogger<ProductDeletedSnapshotConsumer> logger) : IConsumer<ProductDeletedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<ProductDeletedIntegrationEvent> context)
    {
        var msg = context.Message;

        logger.LogInformation("Removing product snapshot for deleted product {ProductId}", msg.ProductId);

        await cache.RemoveAsync(msg.ProductId, context.CancellationToken);
    }
}
