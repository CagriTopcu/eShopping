using Shared.BuildingBlocks.Domain.Primitives;
using Shared.BuildingBlocks.Results;
using Stock.Domain.Errors;
using Stock.Domain.ValueObjects;

namespace Stock.Domain.Entities;

public sealed class StockItem : AggregateRoot<StockItemId>
{
    private StockItem(StockItemId id, Guid productId, int availableQuantity) : base(id)
    {
        ProductId = productId;
        AvailableQuantity = availableQuantity;
    }

    public Guid ProductId { get; private set; }
    public int AvailableQuantity { get; private set; }

    public static StockItem Create(Guid productId, int availableQuantity) =>
        new(StockItemId.New(), productId, availableQuantity);

    public Result SetAvailableQuantity(int quantity)
    {
        if (quantity < 0)
            return StockErrors.InvalidQuantity;

        AvailableQuantity = quantity;
        return Result.Success();
    }

    public Result Reserve(int quantity)
    {
        if (quantity <= 0)
            return StockErrors.InvalidQuantity;

        if (AvailableQuantity < quantity)
            return StockErrors.InsufficientStock(AvailableQuantity);

        AvailableQuantity -= quantity;
        return Result.Success();
    }

    public void Release(int quantity)
    {
        if (quantity > 0)
            AvailableQuantity += quantity;
    }
}
