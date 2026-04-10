using Shared.BuildingBlocks.CQRS;
using UserProfile.Application.DTOs;

namespace UserProfile.Application.Queries.GetProfile;

public sealed record GetProfileQuery(Guid KeycloakUserId) : IQuery<ProfileResponse>;
