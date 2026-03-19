using Order.Domain.ValueObjects;
using Shared.BuildingBlocks.Domain.Events;

namespace Order.Domain.Events;

public sealed record OrderPlacedDomainEvent(OrderId OrderId, Guid CustomerId) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
