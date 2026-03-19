using MassTransit;
using Microsoft.Extensions.Logging;
using Shared.Contracts.Commands.Stock;
using Stock.Application.Abstractions;

namespace Stock.Application.Consumers;

public sealed class ReleaseStockConsumer(
    IStockRepository stockRepository,
    ILogger<ReleaseStockConsumer> logger) : IConsumer<ReleaseStockCommand>
{
    public async Task Consume(ConsumeContext<ReleaseStockCommand> context)
    {
        var command = context.Message;

        foreach (var item in command.Items)
        {
            var stockItem = await stockRepository.GetByProductIdAsync(item.ProductId, context.CancellationToken);
            stockItem?.Release(item.Quantity);
        }

        await stockRepository.SaveChangesAsync(context.CancellationToken);

        logger.LogInformation("Stock released for order {OrderId}", command.OrderId);
    }
}
