using Mapster;
using Microsoft.Extensions.Logging;
using Shared.BuildingBlocks.CQRS;
using Shared.BuildingBlocks.Results;
using UserProfile.Application.Abstractions;
using UserProfile.Application.DTOs;
using UserProfile.Domain.Errors;

namespace UserProfile.Application.Queries.GetProfile;

internal sealed class GetProfileQueryHandler(
    IProfileRepository repository,
    ILogger<GetProfileQueryHandler> logger)
    : IQueryHandler<GetProfileQuery, ProfileResponse>
{
    public async Task<Result<ProfileResponse>> Handle(GetProfileQuery request, CancellationToken cancellationToken)
    {
        var profile = await repository.GetByKeycloakUserIdAsync(request.KeycloakUserId, cancellationToken);

        if (profile is null)
        {
            logger.LogDebug("Profile not found for user {KeycloakUserId}", request.KeycloakUserId);
            return ProfileErrors.NotFound;
        }

        return profile.Adapt<ProfileResponse>();
    }
}
