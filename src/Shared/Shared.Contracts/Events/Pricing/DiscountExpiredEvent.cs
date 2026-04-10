using Shared.Contracts.Events;

namespace Shared.Contracts.Events.Pricing;

public sealed record DiscountExpiredEvent(
    Guid DiscountId,
    string Code) : IntegrationEvent;
