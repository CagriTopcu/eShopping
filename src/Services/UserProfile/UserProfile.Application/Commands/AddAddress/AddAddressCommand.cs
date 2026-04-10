using Shared.BuildingBlocks.CQRS;
using UserProfile.Application.DTOs;

namespace UserProfile.Application.Commands.AddAddress;

public sealed record AddAddressCommand(
    Guid KeycloakUserId,
    string Street,
    string City,
    string State,
    string ZipCode,
    string Country,
    string Label,
    bool IsDefault) : ICommand<AddressResponse>;
