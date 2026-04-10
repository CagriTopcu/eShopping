using FluentAssertions;
using Order.Domain.Entities;
using Order.Domain.Enums;
using Order.Domain.Errors;
using Order.Domain.Events;
using Order.Domain.ValueObjects;

namespace Order.Domain.Tests;

public sealed class OrderTests
{
    private static readonly Address TestAddress = new("123 Main St", "Springfield", "IL", "US", "62701");

    private static readonly (Guid ProductId, string ProductName, decimal UnitPrice, int Quantity)[] ValidItems =
    [
        (Guid.NewGuid(), "Widget", 25.00m, 2),
        (Guid.NewGuid(), "Gadget", 49.99m, 1)
    ];

    // ── Place ───────────────────────────────────────────────────────────

    [Fact]
    public void Place_WithValidArgs_ShouldSucceedWithPendingStatus()
    {
        var result = Entities.Order.Place(Guid.NewGuid(), TestAddress, ValidItems);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(OrderStatus.Pending);
    }

    [Fact]
    public void Place_ShouldRaiseOrderPlacedDomainEvent()
    {
        var customerId = Guid.NewGuid();

        var result = Entities.Order.Place(customerId, TestAddress, ValidItems);

        result.Value.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<OrderPlacedDomainEvent>()
            .Which.CustomerId.Should().Be(customerId);
    }

    [Fact]
    public void Place_ShouldCalculateTotalAmountFromItems()
    {
        // 25 * 2 + 49.99 * 1 = 99.99
        var result = Entities.Order.Place(Guid.NewGuid(), TestAddress, ValidItems);

        result.Value.TotalAmount.Should().Be(99.99m);
    }

    [Fact]
    public void Place_WithEmptyItems_ShouldReturnEmptyItemsError()
    {
        var result = Entities.Order.Place(Guid.NewGuid(), TestAddress, []);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(OrderErrors.EmptyItems);
    }

    [Fact]
    public void Place_WithZeroQuantity_ShouldReturnInvalidQuantityError()
    {
        var items = new[] { (Guid.NewGuid(), "Widget", 25.00m, 0) };

        var result = Entities.Order.Place(Guid.NewGuid(), TestAddress, items);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(OrderErrors.InvalidQuantity);
    }

    [Fact]
    public void Place_WithNegativeQuantity_ShouldReturnInvalidQuantityError()
    {
        var items = new[] { (Guid.NewGuid(), "Widget", 25.00m, -1) };

        var result = Entities.Order.Place(Guid.NewGuid(), TestAddress, items);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(OrderErrors.InvalidQuantity);
    }

    [Fact]
    public void Place_WithZeroPrice_ShouldReturnInvalidPriceError()
    {
        var items = new[] { (Guid.NewGuid(), "Widget", 0m, 1) };

        var result = Entities.Order.Place(Guid.NewGuid(), TestAddress, items);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(OrderErrors.InvalidPrice);
    }

    [Fact]
    public void Place_WithNegativePrice_ShouldReturnInvalidPriceError()
    {
        var items = new[] { (Guid.NewGuid(), "Widget", -5m, 1) };

        var result = Entities.Order.Place(Guid.NewGuid(), TestAddress, items);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(OrderErrors.InvalidPrice);
    }

    // ── Confirm ─────────────────────────────────────────────────────────

    [Fact]
    public void Confirm_WhenPending_ShouldTransitionToConfirmed()
    {
        var order = Entities.Order.Place(Guid.NewGuid(), TestAddress, ValidItems).Value;

        var result = order.Confirm();

        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Confirmed);
        order.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void Confirm_WhenAlreadyCancelled_ShouldReturnAlreadyCancelledError()
    {
        var order = Entities.Order.Place(Guid.NewGuid(), TestAddress, ValidItems).Value;
        order.Cancel();

        var result = order.Confirm();

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(OrderErrors.AlreadyCancelled);
    }

    // ── Cancel ──────────────────────────────────────────────────────────

    [Fact]
    public void Cancel_WhenPending_ShouldTransitionToCancelled()
    {
        var order = Entities.Order.Place(Guid.NewGuid(), TestAddress, ValidItems).Value;

        var result = order.Cancel();

        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Cancelled);
    }

    [Fact]
    public void Cancel_WhenPending_ShouldRaiseOrderCancelledDomainEvent()
    {
        var order = Entities.Order.Place(Guid.NewGuid(), TestAddress, ValidItems).Value;
        order.ClearDomainEvents();

        order.Cancel();

        order.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<OrderCancelledDomainEvent>();
    }

    [Fact]
    public void Cancel_WhenAlreadyCancelled_ShouldReturnAlreadyCancelledError()
    {
        var order = Entities.Order.Place(Guid.NewGuid(), TestAddress, ValidItems).Value;
        order.Cancel();

        var result = order.Cancel();

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(OrderErrors.AlreadyCancelled);
    }

    [Fact]
    public void Cancel_WhenConfirmed_ShouldReturnCannotCancelConfirmedError()
    {
        var order = Entities.Order.Place(Guid.NewGuid(), TestAddress, ValidItems).Value;
        order.Confirm();

        var result = order.Cancel();

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(OrderErrors.CannotCancelConfirmed);
    }

    // ── State transition matrix ─────────────────────────────────────────
    // Pending → Confirmed ✓, Pending → Cancelled ✓
    // Confirmed → Cancelled ✗, Cancelled → Confirmed ✗

    [Fact]
    public void StateTransition_PendingToConfirmedToCancelledIsInvalid()
    {
        var order = Entities.Order.Place(Guid.NewGuid(), TestAddress, ValidItems).Value;
        order.Confirm().IsSuccess.Should().BeTrue();

        order.Cancel().IsFailure.Should().BeTrue("confirmed orders cannot be cancelled");
    }

    [Fact]
    public void StateTransition_PendingToCancelledToConfirmedIsInvalid()
    {
        var order = Entities.Order.Place(Guid.NewGuid(), TestAddress, ValidItems).Value;
        order.Cancel().IsSuccess.Should().BeTrue();

        order.Confirm().IsFailure.Should().BeTrue("cancelled orders cannot be confirmed");
    }
}
