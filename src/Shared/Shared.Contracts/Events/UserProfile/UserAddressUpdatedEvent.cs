using Shared.Contracts.Events;

namespace Shared.Contracts.Events.UserProfile;

public sealed record UserAddressUpdatedEvent(
    Guid UserId,
    string City,
    string Country) : IntegrationEvent;
