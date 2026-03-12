namespace Basket.Domain.Entities;

/// <summary>
/// A line item within a Basket. Not an Entity — it has no independent lifecycle.
/// Items are identified by ProductId within the scope of a Basket.
/// </summary>
public sealed class BasketItem
{
    internal BasketItem(
        Guid productId,
        string productName,
        decimal unitPrice,
        string currency,
        int quantity)
    {
        ProductId = productId;
        ProductName = productName;
        UnitPrice = unitPrice;
        Currency = currency;
        Quantity = quantity;
    }

    public Guid ProductId { get; }
    public string ProductName { get; private set; }
    public decimal UnitPrice { get; private set; }
    public string Currency { get; private set; }
    public int Quantity { get; private set; }

    public decimal LineTotal => UnitPrice * Quantity;

    internal void UpdateFromSnapshot(string productName, decimal unitPrice, string currency)
    {
        ProductName = productName;
        UnitPrice = unitPrice;
        Currency = currency;
    }

    internal void SetQuantity(int quantity) => Quantity = quantity;
}
