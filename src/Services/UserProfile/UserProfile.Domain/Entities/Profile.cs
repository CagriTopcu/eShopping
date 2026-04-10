using Shared.BuildingBlocks.Domain.Primitives;
using Shared.BuildingBlocks.Results;
using UserProfile.Domain.Errors;
using UserProfile.Domain.ValueObjects;

namespace UserProfile.Domain.Entities;

public sealed class Profile : AggregateRoot<UserProfileId>
{
    private readonly List<UserAddress> _addresses = [];

    private Profile() : base(UserProfileId.New())
    {
        FirstName = null!;
        LastName = null!;
        Email = null!;
    }

    private Profile(
        UserProfileId id,
        Guid keycloakUserId,
        string firstName,
        string lastName,
        string email,
        string? phoneNumber) : base(id)
    {
        KeycloakUserId = keycloakUserId;
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        PhoneNumber = phoneNumber;
    }

    public Guid KeycloakUserId { get; private set; }
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string Email { get; private set; }
    public string? PhoneNumber { get; private set; }
    public IReadOnlyList<UserAddress> Addresses => _addresses.AsReadOnly();

    public static Profile Create(
        Guid keycloakUserId,
        string firstName,
        string lastName,
        string email,
        string? phoneNumber = null) =>
        new(UserProfileId.New(), keycloakUserId, firstName, lastName, email, phoneNumber);

    public void Update(string firstName, string lastName, string? phoneNumber)
    {
        FirstName = firstName;
        LastName = lastName;
        PhoneNumber = phoneNumber;
    }

    public Result<UserAddress> AddAddress(Address address, string label, bool isDefault = false)
    {
        if (_addresses.Count >= 10)
            return ProfileErrors.MaxAddressesReached;

        if (isDefault)
            ClearDefaultAddress();

        var userAddress = UserAddress.Create(address, label, isDefault || _addresses.Count == 0);
        _addresses.Add(userAddress);
        return userAddress;
    }

    public Result RemoveAddress(AddressId addressId)
    {
        var address = _addresses.FirstOrDefault(a => a.Id == addressId);
        if (address is null)
            return ProfileErrors.AddressNotFound;

        _addresses.Remove(address);

        if (address.IsDefault && _addresses.Count > 0)
            _addresses[0].IsDefault = true;

        return Result.Success();
    }

    public Result SetDefaultAddress(AddressId addressId)
    {
        var address = _addresses.FirstOrDefault(a => a.Id == addressId);
        if (address is null)
            return ProfileErrors.AddressNotFound;

        ClearDefaultAddress();
        address.IsDefault = true;
        return Result.Success();
    }

    private void ClearDefaultAddress()
    {
        foreach (var addr in _addresses)
            addr.IsDefault = false;
    }
}
