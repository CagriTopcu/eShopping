using Shared.BuildingBlocks.Domain.Events;

namespace Basket.Domain.Events;

public sealed record BasketItemAddedDomainEvent(
    string Username,
    Guid ProductId,
    int Quantity) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
