using Shared.BuildingBlocks.Domain.Events;

namespace Basket.Domain.Events;

public sealed record BasketClearedDomainEvent(string Username) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
