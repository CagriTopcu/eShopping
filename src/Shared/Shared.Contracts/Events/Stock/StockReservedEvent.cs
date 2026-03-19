using Shared.Contracts.Events;

namespace Shared.Contracts.Events.Stock;

public sealed record StockReservedEvent(Guid OrderId) : IntegrationEvent;
