using System.Text.Json;
using FluentAssertions;
using Shared.Contracts.Events;
using Shared.Contracts.Events.Catalog;
using Shared.Contracts.Events.Orders;
using Shared.Contracts.Events.Payment;
using Shared.Contracts.Events.Stock;

namespace Pact.Tests;

/// <summary>
/// Consumer-driven contract tests that verify integration event schemas
/// remain compatible between producer and consumer services.
/// Each test validates that the event can be serialized → deserialized without data loss
/// and that all required properties expected by consumers are present.
/// </summary>
public sealed class IntegrationEventContractTests
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    // ── Catalog → Stock contract ────────────────────────────────────────
    // Producer: Catalog publishes ProductCreatedIntegrationEvent
    // Consumer: Stock.ProductCreatedConsumer needs ProductId, Stock (initial quantity)

    [Fact]
    public void ProductCreatedEvent_ShouldRoundtrip_WithAllStockConsumerFields()
    {
        var original = new ProductCreatedIntegrationEvent(
            Guid.NewGuid(), "Test Product", "Electronics", 99.99m, "USD", 50);

        var deserialized = RoundTrip(original);

        deserialized.ProductId.Should().Be(original.ProductId);
        deserialized.Name.Should().Be(original.Name);
        deserialized.Category.Should().Be(original.Category);
        deserialized.Price.Should().Be(original.Price);
        deserialized.Currency.Should().Be(original.Currency);
        deserialized.Stock.Should().Be(original.Stock);
        AssertBaseEventFields(deserialized);
    }

    // ── Catalog → Basket contract ───────────────────────────────────────
    // Producer: Catalog publishes ProductUpdated/PriceChanged/Deleted
    // Consumer: Basket.ProductSnapshotConsumer needs ProductId, Name, Price, Currency, Category

    [Fact]
    public void ProductUpdatedEvent_ShouldRoundtrip_WithAllBasketConsumerFields()
    {
        var original = new ProductUpdatedIntegrationEvent(
            Guid.NewGuid(), "Updated Product", "Electronics", 129.99m, "USD");

        var deserialized = RoundTrip(original);

        deserialized.ProductId.Should().Be(original.ProductId);
        deserialized.Name.Should().Be(original.Name);
        deserialized.Category.Should().Be(original.Category);
        deserialized.Price.Should().Be(original.Price);
        deserialized.Currency.Should().Be(original.Currency);
        AssertBaseEventFields(deserialized);
    }

    [Fact]
    public void ProductDeletedEvent_ShouldRoundtrip_WithProductId()
    {
        var original = new ProductDeletedIntegrationEvent(Guid.NewGuid());

        var deserialized = RoundTrip(original);

        deserialized.ProductId.Should().Be(original.ProductId);
        AssertBaseEventFields(deserialized);
    }

    // ── Order → Stock contract ──────────────────────────────────────────
    // Producer: OrderSaga publishes OrderPlacedIntegrationEvent
    // Consumer: Stock.ReserveStockConsumer correlates via OrderId

    [Fact]
    public void OrderPlacedEvent_ShouldRoundtrip_WithAllFields()
    {
        var items = new List<OrderItemDto>
        {
            new(Guid.NewGuid(), "Widget", 25.00m, 3),
            new(Guid.NewGuid(), "Gadget", 49.99m, 1)
        };

        var original = new OrderPlacedIntegrationEvent(
            Guid.NewGuid(), Guid.NewGuid(), "testuser", items, 124.99m, DateTimeOffset.UtcNow);

        var deserialized = RoundTrip(original);

        deserialized.OrderId.Should().Be(original.OrderId);
        deserialized.CustomerId.Should().Be(original.CustomerId);
        deserialized.Username.Should().Be(original.Username);
        deserialized.Items.Should().HaveCount(2);
        deserialized.Items[0].ProductId.Should().Be(items[0].ProductId);
        deserialized.Items[0].ProductName.Should().Be("Widget");
        deserialized.Items[0].UnitPrice.Should().Be(25.00m);
        deserialized.Items[0].Quantity.Should().Be(3);
        deserialized.TotalAmount.Should().Be(original.TotalAmount);
        deserialized.PlacedAt.Should().BeCloseTo(original.PlacedAt, TimeSpan.FromSeconds(1));
        AssertBaseEventFields(deserialized);
    }

    [Fact]
    public void OrderConfirmedEvent_ShouldRoundtrip_WithAllFields()
    {
        var original = new OrderConfirmedIntegrationEvent(
            Guid.NewGuid(), Guid.NewGuid(), 199.99m, DateTimeOffset.UtcNow);

        var deserialized = RoundTrip(original);

        deserialized.OrderId.Should().Be(original.OrderId);
        deserialized.CustomerId.Should().Be(original.CustomerId);
        deserialized.TotalAmount.Should().Be(original.TotalAmount);
        deserialized.ConfirmedAt.Should().BeCloseTo(original.ConfirmedAt, TimeSpan.FromSeconds(1));
        AssertBaseEventFields(deserialized);
    }

    [Fact]
    public void OrderCancelledEvent_ShouldRoundtrip_WithAllFields()
    {
        var original = new OrderCancelledIntegrationEvent(
            Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow);

        var deserialized = RoundTrip(original);

        deserialized.OrderId.Should().Be(original.OrderId);
        deserialized.CustomerId.Should().Be(original.CustomerId);
        deserialized.CancelledAt.Should().BeCloseTo(original.CancelledAt, TimeSpan.FromSeconds(1));
        AssertBaseEventFields(deserialized);
    }

    // ── Stock → Order saga contract ─────────────────────────────────────
    // Producer: Stock.ReserveStockConsumer publishes StockReserved/Failed
    // Consumer: OrderSaga correlates via OrderId

    [Fact]
    public void StockReservedEvent_ShouldRoundtrip_WithOrderId()
    {
        var original = new StockReservedEvent(Guid.NewGuid());

        var deserialized = RoundTrip(original);

        deserialized.OrderId.Should().Be(original.OrderId);
        AssertBaseEventFields(deserialized);
    }

    [Fact]
    public void StockReservationFailedEvent_ShouldRoundtrip_WithOrderIdAndReason()
    {
        var original = new StockReservationFailedEvent(Guid.NewGuid(), "Insufficient stock for product XYZ");

        var deserialized = RoundTrip(original);

        deserialized.OrderId.Should().Be(original.OrderId);
        deserialized.Reason.Should().Be(original.Reason);
        AssertBaseEventFields(deserialized);
    }

    // ── Payment → Order saga contract ───────────────────────────────────
    // Producer: Payment.ProcessPaymentConsumer publishes PaymentReserved/Failed
    // Consumer: OrderSaga correlates via OrderId

    [Fact]
    public void PaymentReservedEvent_ShouldRoundtrip_WithAllFields()
    {
        var original = new PaymentReservedIntegrationEvent(
            Guid.NewGuid(), Guid.NewGuid(), "txn-stripe-12345");

        var deserialized = RoundTrip(original);

        deserialized.OrderId.Should().Be(original.OrderId);
        deserialized.PaymentId.Should().Be(original.PaymentId);
        deserialized.TransactionId.Should().Be(original.TransactionId);
        AssertBaseEventFields(deserialized);
    }

    [Fact]
    public void PaymentFailedEvent_ShouldRoundtrip_WithOrderIdAndReason()
    {
        var original = new PaymentFailedIntegrationEvent(Guid.NewGuid(), "Card declined");

        var deserialized = RoundTrip(original);

        deserialized.OrderId.Should().Be(original.OrderId);
        deserialized.Reason.Should().Be(original.Reason);
        AssertBaseEventFields(deserialized);
    }

    // ── Stock → Notification contract ───────────────────────────────────
    // Producer: Stock.ReserveStockConsumer publishes LowStockWarningEvent
    // Consumer: Notification.LowStockNotificationConsumer

    [Fact]
    public void LowStockWarningEvent_ShouldRoundtrip_WithAllFields()
    {
        var original = new LowStockWarningEvent(Guid.NewGuid(), 5, 10);

        var deserialized = RoundTrip(original);

        deserialized.ProductId.Should().Be(original.ProductId);
        deserialized.RemainingQuantity.Should().Be(original.RemainingQuantity);
        deserialized.Threshold.Should().Be(original.Threshold);
        AssertBaseEventFields(deserialized);
    }

    // ── Version field contract ──────────────────────────────────────────

    [Fact]
    public void AllEvents_ShouldDefaultToVersion1()
    {
        var events = new IIntegrationEvent[]
        {
            new ProductCreatedIntegrationEvent(Guid.NewGuid(), "P", "C", 1m, "USD", 1),
            new ProductUpdatedIntegrationEvent(Guid.NewGuid(), "P", "C", 1m, "USD"),
            new ProductDeletedIntegrationEvent(Guid.NewGuid()),
            new OrderPlacedIntegrationEvent(Guid.NewGuid(), Guid.NewGuid(), "u", [], 0m, DateTimeOffset.UtcNow),
            new OrderConfirmedIntegrationEvent(Guid.NewGuid(), Guid.NewGuid(), 0m, DateTimeOffset.UtcNow),
            new OrderCancelledIntegrationEvent(Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow),
            new StockReservedEvent(Guid.NewGuid()),
            new StockReservationFailedEvent(Guid.NewGuid(), "r"),
            new PaymentReservedIntegrationEvent(Guid.NewGuid(), Guid.NewGuid(), "t"),
            new PaymentFailedIntegrationEvent(Guid.NewGuid(), "r"),
            new LowStockWarningEvent(Guid.NewGuid(), 5, 10)
        };

        foreach (var e in events)
        {
            e.Version.Should().Be(1, $"{e.EventType} should default to version 1");
        }
    }

    // ── Forward compatibility: extra fields are ignored ──────────────────

    [Fact]
    public void Deserializing_EventWithExtraFields_ShouldNotFail()
    {
        // Simulate a producer adding a new field that the consumer doesn't know about
        var json = """{"orderId":"00000000-0000-0000-0000-000000000001","id":"00000000-0000-0000-0000-000000000002","occurredOn":"2026-01-01T00:00:00Z","eventType":"StockReservedEvent","version":1,"newFutureField":"value"}""";

        var act = () => JsonSerializer.Deserialize<StockReservedEvent>(json, Options);

        act.Should().NotThrow("consumers must tolerate extra fields from newer producers");
    }

    [Fact]
    public void AllEvents_ShouldHaveUniqueEventTypeFromClassName()
    {
        var events = new IIntegrationEvent[]
        {
            new ProductCreatedIntegrationEvent(Guid.NewGuid(), "P", "C", 1m, "USD", 1),
            new OrderPlacedIntegrationEvent(Guid.NewGuid(), Guid.NewGuid(), "u", [], 0m, DateTimeOffset.UtcNow),
            new StockReservedEvent(Guid.NewGuid()),
            new PaymentReservedIntegrationEvent(Guid.NewGuid(), Guid.NewGuid(), "t"),
            new LowStockWarningEvent(Guid.NewGuid(), 5, 10)
        };

        foreach (var e in events)
        {
            e.EventType.Should().Be(e.GetType().Name, "EventType should match the class name for routing");
        }
    }

    // ── Helpers ─────────────────────────────────────────────────────────

    private static T RoundTrip<T>(T original) where T : class
    {
        var json = JsonSerializer.Serialize(original, Options);
        var deserialized = JsonSerializer.Deserialize<T>(json, Options);
        deserialized.Should().NotBeNull($"deserialization of {typeof(T).Name} should not return null");
        return deserialized!;
    }

    private static void AssertBaseEventFields(IIntegrationEvent e)
    {
        e.Id.Should().NotBe(Guid.Empty, "event Id must be set");
        e.OccurredOn.Should().BeAfter(DateTime.MinValue, "OccurredOn must be set");
        e.EventType.Should().NotBeNullOrWhiteSpace("EventType must be set");
        e.Version.Should().BeGreaterThan(0, "Version must be positive");
    }
}
