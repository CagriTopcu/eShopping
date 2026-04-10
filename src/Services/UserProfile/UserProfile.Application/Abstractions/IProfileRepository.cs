using UserProfile.Domain.Entities;

namespace UserProfile.Application.Abstractions;

public interface IProfileRepository
{
    Task<Profile?> GetByKeycloakUserIdAsync(Guid keycloakUserId, CancellationToken ct = default);
    Task<Profile?> GetByIdAsync(Guid profileId, CancellationToken ct = default);
    Task AddAsync(Profile profile, CancellationToken ct = default);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
