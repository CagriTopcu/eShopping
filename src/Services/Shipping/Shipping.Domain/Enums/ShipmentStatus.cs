namespace Shipping.Domain.Enums;

public enum ShipmentStatus
{
    Created = 0,
    Processing = 1,
    Shipped = 2,
    InTransit = 3,
    Delivered = 4,
    Failed = 5
}
