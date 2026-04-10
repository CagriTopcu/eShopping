namespace Shipping.Domain.ValueObjects;

public sealed record ShipmentId(Guid Value)
{
    public static ShipmentId New() => new(Guid.NewGuid());
}
