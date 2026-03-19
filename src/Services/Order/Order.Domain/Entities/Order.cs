using Order.Domain.Enums;
using Order.Domain.Errors;
using Order.Domain.Events;
using Order.Domain.ValueObjects;
using Shared.BuildingBlocks.Domain.Primitives;
using Shared.BuildingBlocks.Results;

namespace Order.Domain.Entities;

public sealed class Order : AggregateRoot<OrderId>, IAuditableEntity, ISoftDeletable
{
    private readonly List<OrderItem> _items = [];

    private Order() : base(OrderId.New()) { }

    private Order(OrderId id, Guid customerId, Address shippingAddress) : base(id)
    {
        CustomerId = customerId;
        ShippingAddress = shippingAddress;
        Status = OrderStatus.Pending;
        PlacedAt = DateTime.UtcNow;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid CustomerId { get; private set; }
    public OrderStatus Status { get; private set; }
    public Address ShippingAddress { get; private set; } = null!;
    public IReadOnlyList<OrderItem> Items => _items.AsReadOnly();
    public decimal TotalAmount => _items.Sum(i => i.LineTotal);
    public DateTime PlacedAt { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    public static Result<Order> Place(
        Guid customerId,
        Address shippingAddress,
        IEnumerable<(Guid ProductId, string ProductName, decimal UnitPrice, int Quantity)> items)
    {
        var itemList = items.ToList();

        if (itemList.Count == 0)
            return OrderErrors.EmptyItems;

        foreach (var item in itemList)
        {
            if (item.Quantity <= 0) return OrderErrors.InvalidQuantity;
            if (item.UnitPrice <= 0) return OrderErrors.InvalidPrice;
        }

        var order = new Order(OrderId.New(), customerId, shippingAddress);

        foreach (var item in itemList)
            order._items.Add(new OrderItem(item.ProductId, item.ProductName, item.UnitPrice, item.Quantity));

        order.RaiseDomainEvent(new OrderPlacedDomainEvent(order.Id, customerId));

        return order;
    }

    public Result Confirm()
    {
        if (Status == OrderStatus.Cancelled)
            return OrderErrors.AlreadyCancelled;

        Status = OrderStatus.Confirmed;
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public Result Cancel()
    {
        if (Status == OrderStatus.Cancelled)
            return OrderErrors.AlreadyCancelled;

        if (Status == OrderStatus.Confirmed)
            return OrderErrors.CannotCancelConfirmed;

        Status = OrderStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
        RaiseDomainEvent(new OrderCancelledDomainEvent(Id, CustomerId));
        return Result.Success();
    }
}
