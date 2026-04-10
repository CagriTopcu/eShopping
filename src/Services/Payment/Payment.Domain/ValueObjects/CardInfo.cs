using Shared.BuildingBlocks.Domain.Primitives;

namespace Payment.Domain.ValueObjects;

public sealed class CardInfo : ValueObject
{
    private CardInfo() { }

    private CardInfo(string maskedNumber, string expiryMonth, string expiryYear, string cardHolderName)
    {
        MaskedNumber = maskedNumber;
        ExpiryMonth = expiryMonth;
        ExpiryYear = expiryYear;
        CardHolderName = cardHolderName;
    }

    public string MaskedNumber { get; private set; } = string.Empty;
    public string ExpiryMonth { get; private set; } = string.Empty;
    public string ExpiryYear { get; private set; } = string.Empty;
    public string CardHolderName { get; private set; } = string.Empty;

    public static CardInfo CreateAnonymous() =>
        new("****0000", "00", "00", "SAGA");

    public static CardInfo Create(string? cardNumber, string? expiryMonth, string? expiryYear, string? cardHolderName)
    {
        var masked = string.IsNullOrWhiteSpace(cardNumber) || cardNumber.Length < 4
            ? "****0000"
            : $"****{cardNumber[^4..]}";

        return new CardInfo(
            masked,
            expiryMonth ?? "00",
            expiryYear ?? "00",
            cardHolderName ?? "N/A");
    }

    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return MaskedNumber;
        yield return ExpiryMonth;
        yield return ExpiryYear;
        yield return CardHolderName;
    }
}
