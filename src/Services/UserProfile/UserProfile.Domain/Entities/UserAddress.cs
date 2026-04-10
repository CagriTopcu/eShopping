using Shared.BuildingBlocks.Domain.Primitives;
using UserProfile.Domain.ValueObjects;

namespace UserProfile.Domain.Entities;

public sealed class UserAddress : Entity<AddressId>
{
    private UserAddress() : base(AddressId.New())
    {
        Address = null!;
        Label = null!;
    }

    private UserAddress(AddressId id, Address address, string label, bool isDefault) : base(id)
    {
        Address = address;
        Label = label;
        IsDefault = isDefault;
    }

    public Address Address { get; private set; }
    public string Label { get; private set; }
    public bool IsDefault { get; internal set; }

    public static UserAddress Create(Address address, string label, bool isDefault = false) =>
        new(AddressId.New(), address, label, isDefault);

    public void Update(Address address, string label)
    {
        Address = address;
        Label = label;
    }
}
