using Shared.Contracts.Events;

namespace Shared.Contracts.Events.Catalog;

public sealed record ProductUpdatedIntegrationEvent(
    Guid ProductId,
    string Name,
    string Category,
    decimal Price,
    string Currency) : IntegrationEvent;
