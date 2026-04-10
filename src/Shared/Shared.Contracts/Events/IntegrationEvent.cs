namespace Shared.Contracts.Events;

public abstract record IntegrationEvent : IIntegrationEvent
{
    protected IntegrationEvent()
    {
        Id = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
        EventType = GetType().Name;
    }

    public Guid Id { get; init; }
    public DateTime OccurredOn { get; init; }
    public string EventType { get; init; }
    public int Version { get; init; } = 1;
}
