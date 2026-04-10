using Shared.BuildingBlocks.Domain.Primitives;

namespace UserProfile.Domain.ValueObjects;

public sealed class Address : ValueObject
{
    private Address() { }

    public string Street { get; private set; } = default!;
    public string City { get; private set; } = default!;
    public string State { get; private set; } = default!;
    public string ZipCode { get; private set; } = default!;
    public string Country { get; private set; } = default!;

    public static Address Create(string street, string city, string state, string zipCode, string country) =>
        new()
        {
            Street = street,
            City = city,
            State = state,
            ZipCode = zipCode,
            Country = country
        };

    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return Street;
        yield return City;
        yield return State;
        yield return ZipCode;
        yield return Country;
    }
}
