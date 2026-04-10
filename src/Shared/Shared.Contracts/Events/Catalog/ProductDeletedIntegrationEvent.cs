using Shared.Contracts.Events;

namespace Shared.Contracts.Events.Catalog;

public sealed record ProductDeletedIntegrationEvent(
    Guid ProductId) : IntegrationEvent;
