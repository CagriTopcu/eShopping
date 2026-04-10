using Shared.BuildingBlocks.CQRS;

namespace UserProfile.Application.Commands.RemoveAddress;

public sealed record RemoveAddressCommand(Guid KeycloakUserId, Guid AddressId) : ICommand;
