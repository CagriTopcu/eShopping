using Mapster;
using Microsoft.Extensions.Logging;
using Shared.BuildingBlocks.CQRS;
using Shared.BuildingBlocks.Results;
using UserProfile.Application.Abstractions;
using UserProfile.Application.DTOs;
using UserProfile.Domain.Errors;

namespace UserProfile.Application.Commands.UpdateProfile;

internal sealed class UpdateProfileCommandHandler(
    IProfileRepository repository,
    ILogger<UpdateProfileCommandHandler> logger)
    : ICommandHandler<UpdateProfileCommand, ProfileResponse>
{
    public async Task<Result<ProfileResponse>> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        var profile = await repository.GetByKeycloakUserIdAsync(request.KeycloakUserId, cancellationToken);
        if (profile is null)
            return ProfileErrors.NotFound;

        profile.Update(request.FirstName, request.LastName, request.PhoneNumber);
        await repository.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Profile updated for user {KeycloakUserId}", request.KeycloakUserId);

        return profile.Adapt<ProfileResponse>();
    }
}
