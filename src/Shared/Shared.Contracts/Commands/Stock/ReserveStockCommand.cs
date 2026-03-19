namespace Shared.Contracts.Commands.Stock;

public sealed record ReserveStockCommand(
    Guid OrderId,
    IReadOnlyList<StockCommandItem> Items);

public sealed record StockCommandItem(Guid ProductId, int Quantity);
