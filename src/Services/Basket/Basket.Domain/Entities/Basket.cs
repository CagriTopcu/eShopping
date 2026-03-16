using Basket.Domain.Errors;
using Basket.Domain.Events;
using Basket.Domain.ValueObjects;
using Shared.BuildingBlocks.Domain.Primitives;
using Shared.BuildingBlocks.Results;

namespace Basket.Domain.Entities;

public sealed class Basket : AggregateRoot<BasketId>
{
    public const int MaxItemsPerBasket = 50;
    public const int MaxQuantityPerItem = 99;

    private readonly List<BasketItem> _items = [];

    private Basket(BasketId id) : base(id) { }

    public IReadOnlyList<BasketItem> Items => _items.AsReadOnly();

    public decimal TotalPrice => _items.Sum(i => i.LineTotal);

    public static Basket Create(string username) =>
        new(BasketId.From(username));

    public static Basket Reconstitute(string username, IEnumerable<BasketItem> items)
    {
        var basket = new Basket(BasketId.From(username));
        basket._items.AddRange(items);
        return basket;
    }

    /// <param name="availableStock">
    /// Live stock from StockService. Pass <c>null</c> when no stock record exists —
    /// Basket treats missing stock data as unconstrained (permissive soft-check).
    /// The Order saga performs the hard atomic reservation at checkout.
    /// </param>
    public Result AddOrUpdateItem(
        Guid productId,
        string productName,
        decimal unitPrice,
        string currency,
        int quantity,
        int? availableStock)
    {
        if (quantity <= 0)
            return BasketErrors.InvalidQuantity;

        if (quantity > MaxQuantityPerItem)
            return BasketErrors.InsufficientStock(MaxQuantityPerItem);

        if (availableStock.HasValue)
        {
            if (availableStock.Value == 0)
                return BasketErrors.OutOfStock;

            if (quantity > availableStock.Value)
                return BasketErrors.InsufficientStock(availableStock.Value);
        }

        var existing = _items.FirstOrDefault(i => i.ProductId == productId);

        if (existing is not null)
        {
            existing.UpdateFromSnapshot(productName, unitPrice, currency);
            existing.SetQuantity(quantity);
        }
        else
        {
            if (_items.Count >= MaxItemsPerBasket)
                return BasketErrors.BasketFull;

            if (_items.Count > 0 && _items[0].Currency != currency)
                return BasketErrors.CurrencyMismatch;

            _items.Add(new BasketItem(productId, productName, unitPrice, currency, quantity));
        }

        RaiseDomainEvent(new BasketItemAddedDomainEvent(Id.Value, productId, quantity));
        return Result.Success();
    }

    public Result RemoveItem(Guid productId)
    {
        var item = _items.FirstOrDefault(i => i.ProductId == productId);

        if (item is null)
            return BasketErrors.ItemNotFound;

        _items.Remove(item);
        RaiseDomainEvent(new BasketItemRemovedDomainEvent(Id.Value, productId));

        return Result.Success();
    }

    public Result Clear()
    {
        _items.Clear();
        RaiseDomainEvent(new BasketClearedDomainEvent(Id.Value));
        return Result.Success();
    }
}
