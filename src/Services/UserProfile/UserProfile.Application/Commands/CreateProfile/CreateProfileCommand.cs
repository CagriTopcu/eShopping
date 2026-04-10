using Shared.BuildingBlocks.CQRS;
using UserProfile.Application.DTOs;

namespace UserProfile.Application.Commands.CreateProfile;

public sealed record CreateProfileCommand(
    Guid KeycloakUserId,
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber) : ICommand<ProfileResponse>;
