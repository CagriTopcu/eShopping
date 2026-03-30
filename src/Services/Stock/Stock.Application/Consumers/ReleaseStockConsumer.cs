using MassTransit;
using Microsoft.Extensions.Logging;
using Shared.Contracts.Commands.Stock;
using Shared.Contracts.Events.Stock;
using Stock.Application.Abstractions;

namespace Stock.Application.Consumers;

public sealed class ReleaseStockConsumer(
    IStockRepository stockRepository,
    IPublishEndpoint publishEndpoint,
    ILogger<ReleaseStockConsumer> logger) : IConsumer<ReleaseStockCommand>
{
    public async Task Consume(ConsumeContext<ReleaseStockCommand> context)
    {
        var command = context.Message;

        foreach (var item in command.Items)
        {
            var stockItem = await stockRepository.GetByProductIdAsync(item.ProductId, context.CancellationToken);

            if (stockItem is null)
            {
                logger.LogWarning(
                    "Stock record not found for product {ProductId} during release for order {OrderId}. Skipping.",
                    item.ProductId, command.OrderId);
                continue;
            }

            stockItem.Release(item.Quantity);
        }

        await stockRepository.SaveChangesAsync(context.CancellationToken);

        logger.LogInformation("Stock released for order {OrderId}", command.OrderId);

        await publishEndpoint.Publish(
            new StockUpdatedIntegrationEvent(
                command.Items.Select(i => new StockUpdateItem(i.ProductId, i.Quantity)).ToList()),
            context.CancellationToken);
    }
}
