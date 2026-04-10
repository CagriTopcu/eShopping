using Shared.Contracts.Events;

namespace Shared.Contracts.Events.Pricing;

public sealed record DiscountCreatedEvent(
    Guid DiscountId,
    string Code,
    decimal Percentage,
    DateTimeOffset ValidFrom,
    DateTimeOffset ValidTo) : IntegrationEvent;
