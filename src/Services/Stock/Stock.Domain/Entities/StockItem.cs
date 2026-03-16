using Shared.BuildingBlocks.Domain.Primitives;
using Shared.BuildingBlocks.Results;
using Stock.Domain.Errors;
using Stock.Domain.ValueObjects;

namespace Stock.Domain.Entities;

/// <summary>
/// Represents the inventory record for a single product.
/// Currently handles availability queries (soft checks for Basket service).
///
/// Future extension points (not implemented — add when Order saga needs them):
///   - ReservedQuantity: tracks quantity held by pending orders
///   - Reserve(int quantity, Guid reservationId): atomic reservation for checkout
///   - Release(Guid reservationId): release reservation on order cancellation
///   - Confirm(Guid reservationId): deduct confirmed quantity on order completion
/// </summary>
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
}
