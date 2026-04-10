using Microsoft.EntityFrameworkCore;
using UserProfile.Application.Abstractions;
using UserProfile.Domain.Entities;
using UserProfile.Domain.ValueObjects;

namespace UserProfile.Infrastructure.Persistence;

internal sealed class ProfileRepository(UserProfileDbContext dbContext) : IProfileRepository
{
    public async Task<Profile?> GetByKeycloakUserIdAsync(Guid keycloakUserId, CancellationToken ct = default) =>
        await dbContext.Profiles.FirstOrDefaultAsync(p => p.KeycloakUserId == keycloakUserId, ct);

    public async Task<Profile?> GetByIdAsync(Guid profileId, CancellationToken ct = default) =>
        await dbContext.Profiles.FirstOrDefaultAsync(p => p.Id == new UserProfileId(profileId), ct);

    public async Task AddAsync(Profile profile, CancellationToken ct = default) =>
        await dbContext.Profiles.AddAsync(profile, ct);

    public async Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        await dbContext.SaveChangesAsync(ct);
}
