using Shared.Contracts.Events;

namespace Shared.Contracts.Events.Stock;

public sealed record StockReservationFailedEvent(Guid OrderId, string Reason) : IntegrationEvent;
