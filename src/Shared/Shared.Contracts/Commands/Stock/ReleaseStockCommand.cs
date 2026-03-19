namespace Shared.Contracts.Commands.Stock;

public sealed record ReleaseStockCommand(
    Guid OrderId,
    IReadOnlyList<StockCommandItem> Items);
