using Mapster;
using Microsoft.Extensions.Logging;
using Shared.BuildingBlocks.CQRS;
using Shared.BuildingBlocks.Results;
using UserProfile.Application.Abstractions;
using UserProfile.Application.DTOs;
using UserProfile.Domain.Entities;
using UserProfile.Domain.Errors;

namespace UserProfile.Application.Commands.CreateProfile;

internal sealed class CreateProfileCommandHandler(
    IProfileRepository repository,
    ILogger<CreateProfileCommandHandler> logger)
    : ICommandHandler<CreateProfileCommand, ProfileResponse>
{
    public async Task<Result<ProfileResponse>> Handle(CreateProfileCommand request, CancellationToken cancellationToken)
    {
        var existing = await repository.GetByKeycloakUserIdAsync(request.KeycloakUserId, cancellationToken);
        if (existing is not null)
            return ProfileErrors.AlreadyExists;

        var profile = Profile.Create(
            request.KeycloakUserId,
            request.FirstName,
            request.LastName,
            request.Email,
            request.PhoneNumber);

        await repository.AddAsync(profile, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Profile created for user {KeycloakUserId}", request.KeycloakUserId);

        return profile.Adapt<ProfileResponse>();
    }
}
