using Shared.Contracts.Events;

namespace Shared.Contracts.Events.Pricing;

public sealed record DiscountAppliedEvent(
    Guid OrderId,
    string DiscountCode,
    decimal DiscountAmount,
    decimal OriginalTotal,
    decimal DiscountedTotal) : IntegrationEvent;
