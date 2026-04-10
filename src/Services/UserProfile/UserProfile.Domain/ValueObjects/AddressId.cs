namespace UserProfile.Domain.ValueObjects;

public sealed record AddressId(Guid Value)
{
    public static AddressId New() => new(Guid.NewGuid());
}
