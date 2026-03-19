using Order.Domain.ValueObjects;
using Shared.BuildingBlocks.Domain.Primitives;

namespace Order.Domain.Entities;

public sealed class OrderItem : Entity<OrderItemId>
{
    private OrderItem() : base(OrderItemId.New()) { }

    internal OrderItem(Guid productId, string productName, decimal unitPrice, int quantity)
        : base(OrderItemId.New())
    {
        ProductId = productId;
        ProductName = productName;
        UnitPrice = unitPrice;
        Quantity = quantity;
    }

    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; } = string.Empty;
    public decimal UnitPrice { get; private set; }
    public int Quantity { get; private set; }
    public decimal LineTotal => UnitPrice * Quantity;
}
