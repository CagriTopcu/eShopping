using Shared.BuildingBlocks.Domain.Primitives;
using Shared.BuildingBlocks.Results;
using Shipping.Domain.Enums;
using Shipping.Domain.Errors;
using Shipping.Domain.ValueObjects;

namespace Shipping.Domain.Entities;

public sealed class Shipment : AggregateRoot<ShipmentId>
{
    private Shipment() : base(ShipmentId.New()) { }

    private Shipment(
        ShipmentId id,
        Guid orderId,
        Guid customerId,
        decimal orderTotal,
        ShipmentStatus status) : base(id)
    {
        OrderId = orderId;
        CustomerId = customerId;
        OrderTotal = orderTotal;
        Status = status;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public Guid OrderId { get; private set; }
    public Guid CustomerId { get; private set; }
    public decimal OrderTotal { get; private set; }
    public ShipmentStatus Status { get; private set; }
    public string? TrackingNumber { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? ShippedAt { get; private set; }
    public DateTimeOffset? DeliveredAt { get; private set; }

    public static Shipment Create(Guid orderId, Guid customerId, decimal orderTotal) =>
        new(ShipmentId.New(), orderId, customerId, orderTotal, ShipmentStatus.Created);

    public Result MarkShipped(string trackingNumber)
    {
        if (Status is not (ShipmentStatus.Created or ShipmentStatus.Processing))
            return ShipmentErrors.InvalidStatusTransition(Status, ShipmentStatus.Shipped);

        Status = ShipmentStatus.Shipped;
        TrackingNumber = trackingNumber;
        ShippedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }

    public Result MarkDelivered()
    {
        if (Status is not (ShipmentStatus.Shipped or ShipmentStatus.InTransit))
            return ShipmentErrors.InvalidStatusTransition(Status, ShipmentStatus.Delivered);

        Status = ShipmentStatus.Delivered;
        DeliveredAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }

    public Result MarkFailed()
    {
        if (Status is ShipmentStatus.Delivered)
            return ShipmentErrors.InvalidStatusTransition(Status, ShipmentStatus.Failed);

        Status = ShipmentStatus.Failed;
        return Result.Success();
    }
}
