using Shared.BuildingBlocks.Domain.Primitives;
using Shared.BuildingBlocks.Guards;

namespace Basket.Domain.ValueObjects;

public sealed class BasketId : ValueObject
{
    private BasketId(string value) => Value = value;

    public string Value { get; }

    public static BasketId From(string username) =>
        new(Guard.AgainstNullOrWhiteSpace(username, nameof(username)));

    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return Value;
    }
}
