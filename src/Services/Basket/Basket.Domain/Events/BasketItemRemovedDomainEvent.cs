using Shared.BuildingBlocks.Domain.Events;

namespace Basket.Domain.Events;

public sealed record BasketItemRemovedDomainEvent(
    string Username,
    Guid ProductId) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
