namespace Order.Domain.ValueObjects;

public sealed record Address(
    string Street,
    string City,
    string State,
    string Country,
    string ZipCode);
