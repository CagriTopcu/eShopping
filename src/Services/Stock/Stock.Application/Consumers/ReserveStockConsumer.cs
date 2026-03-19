using MassTransit;
using Microsoft.Extensions.Logging;
using Shared.Contracts.Commands.Stock;
using Shared.Contracts.Events.Stock;
using Stock.Application.Abstractions;

namespace Stock.Application.Consumers;

public sealed class ReserveStockConsumer(
    IStockRepository stockRepository,
    IPublishEndpoint publishEndpoint,
    ILogger<ReserveStockConsumer> logger) : IConsumer<ReserveStockCommand>
{
    public async Task Consume(ConsumeContext<ReserveStockCommand> context)
    {
        var command = context.Message;
        var reservedItems = new List<(Guid ProductId, int Quantity)>();

        foreach (var item in command.Items)
        {
            var stockItem = await stockRepository.GetByProductIdAsync(item.ProductId, context.CancellationToken);

            if (stockItem is null)
            {
                logger.LogWarning("Stock not found for product {ProductId} while reserving order {OrderId}",
                    item.ProductId, command.OrderId);

                await RollbackAndFail(reservedItems, command.OrderId,
                    $"Stock record not found for product {item.ProductId}", context.CancellationToken);
                return;
            }

            var result = stockItem.Reserve(item.Quantity);

            if (result.IsFailure)
            {
                logger.LogWarning("Failed to reserve stock for product {ProductId} on order {OrderId}: {Reason}",
                    item.ProductId, command.OrderId, result.Error.Description);

                await RollbackAndFail(reservedItems, command.OrderId,
                    result.Error.Description, context.CancellationToken);
                return;
            }

            reservedItems.Add((item.ProductId, item.Quantity));
        }

        await stockRepository.SaveChangesAsync(context.CancellationToken);

        logger.LogInformation("Stock reserved for order {OrderId} — {Count} item(s)", command.OrderId, reservedItems.Count);

        await publishEndpoint.Publish(new StockReservedEvent(command.OrderId), context.CancellationToken);
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
            item?.Release(quantity);
        }

        if (reservedItems.Count > 0)
            await stockRepository.SaveChangesAsync(ct);

        await publishEndpoint.Publish(new StockReservationFailedEvent(orderId, reason), ct);
    }
}
