using Shared.Contracts.Events;

namespace Shared.Contracts.Events.Stock;

public sealed record LowStockWarningEvent(
    Guid ProductId,
    int RemainingQuantity,
    int Threshold) : IntegrationEvent;
