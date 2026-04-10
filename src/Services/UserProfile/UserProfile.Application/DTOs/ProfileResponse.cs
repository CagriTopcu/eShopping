namespace UserProfile.Application.DTOs;

public sealed record ProfileResponse(
    Guid Id,
    Guid KeycloakUserId,
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber,
    IReadOnlyList<AddressResponse> Addresses);

public sealed record AddressResponse(
    Guid Id,
    string Street,
    string City,
    string State,
    string ZipCode,
    string Country,
    string Label,
    bool IsDefault);
