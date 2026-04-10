using Shared.BuildingBlocks.CQRS;
using UserProfile.Application.DTOs;

namespace UserProfile.Application.Commands.UpdateProfile;

public sealed record UpdateProfileCommand(
    Guid KeycloakUserId,
    string FirstName,
    string LastName,
    string? PhoneNumber) : ICommand<ProfileResponse>;
