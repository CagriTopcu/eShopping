using System.Diagnostics;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Shared.BuildingBlocks.Observability;
using Shared.Contracts.Commands.Stock;
using Shared.Contracts.Events.Stock;
using Stock.Application.Abstractions;


namespace Stock.Application.Consumers;

public sealed class ReserveStockConsumer(
    IStockRepository stockRepository,
    IPublishEndpoint publishEndpoint,
    IConfiguration configuration,
    BusinessMetrics metrics,
    ILogger<ReserveStockConsumer> logger) : IConsumer<ReserveStockCommand>
{
    public async Task Consume(ConsumeContext<ReserveStockCommand> context)
    {
        using var activity = DiagnosticConfig.Source.StartActivity("stock.reserve");
        var command = context.Message;
        activity?.SetTag("order.id", command.OrderId.ToString());
        activity?.SetTag("stock.items.count", command.Items.Count);

        var reservedItems = new List<(Guid ProductId, int Quantity)>();
        var stopwatch = Stopwatch.StartNew();

        foreach (var item in command.Items)
        {
            var stockItem = await stockRepository.GetByProductIdAsync(item.ProductId, context.CancellationToken);

            if (stockItem is null)
            {
                logger.LogWarning("Stock not found for product {ProductId} while reserving order {OrderId}",
                    item.ProductId, command.OrderId);

                metrics.StockReservationFailed("product_not_found");
                await RollbackAndFail(reservedItems, command.OrderId,
                    $"Stock record not found for product {item.ProductId}", context.CancellationToken);
                return;
            }

            var result = stockItem.Reserve(item.Quantity);

            if (result.IsFailure)
            {
                logger.LogWarning("Failed to reserve stock for product {ProductId} on order {OrderId}: {Reason}",
                    item.ProductId, command.OrderId, result.Error.Description);

                metrics.StockReservationFailed("insufficient_stock");
                await RollbackAndFail(reservedItems, command.OrderId,
                    result.Error.Description, context.CancellationToken);
                return;
            }

            reservedItems.Add((item.ProductId, item.Quantity));
        }

        await stockRepository.SaveChangesAsync(context.CancellationToken);

        stopwatch.Stop();
        metrics.RecordStockReservationDuration(stopwatch.Elapsed.TotalMilliseconds);

        logger.LogInformation("Stock reserved for order {OrderId} — {Count} item(s)", command.OrderId, reservedItems.Count);

        await publishEndpoint.Publish(new StockReservedEvent(command.OrderId), context.CancellationToken);

        await publishEndpoint.Publish(
            new StockUpdatedIntegrationEvent(
                reservedItems.Select(i => new StockUpdateItem(i.ProductId, -i.Quantity)).ToList()),
            context.CancellationToken);

        var lowStockThreshold = configuration.GetValue("Stock:LowStockThreshold", 10);

        foreach (var (productId, _) in reservedItems)
        {
            var item = await stockRepository.GetByProductIdAsync(productId, context.CancellationToken);

            if (item is not null && item.AvailableQuantity <= lowStockThreshold && item.AvailableQuantity >= 0)
            {
                metrics.StockLowWarning(productId, item.AvailableQuantity);
                await publishEndpoint.Publish(
                    new LowStockWarningEvent(productId, item.AvailableQuantity, lowStockThreshold),
                    context.CancellationToken);

                logger.LogWarning(
                    "Low stock alert for product {ProductId}: {Remaining} remaining (threshold: {Threshold})",
                    productId, item.AvailableQuantity, lowStockThreshold);
            }
        }
    }

    private async Task RollbackAndFail(
        List<(Guid ProductId, int Quantity)> reservedItems,
        Guid orderId,
        string reason,
        CancellationToken ct)
    {
        foreach (var (productId, quantity) in reservedItems)
        {
            var item = await stockRepository.GetByProductIdAsync(productId, ct);

            if (item is null)
            {
                logger.LogWarning(
                    "Stock record not found for product {ProductId} during rollback for order {OrderId}. Rollback incomplete.",
                    productId, orderId);
                continue;
            }

            item.Release(quantity);
        }

        if (reservedItems.Count > 0)
            await stockRepository.SaveChangesAsync(ct);

        await publishEndpoint.Publish(new StockReservationFailedEvent(orderId, reason), ct);
    }
}
