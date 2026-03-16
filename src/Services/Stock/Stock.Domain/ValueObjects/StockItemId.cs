namespace Stock.Domain.ValueObjects;

public sealed record StockItemId(Guid Value)
{
    public static StockItemId New() => new(Guid.NewGuid());
}
