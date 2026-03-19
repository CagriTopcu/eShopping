using Shared.BuildingBlocks.Results;

namespace Order.Domain.Errors;

public static class OrderErrors
{
    public static readonly Error NotFound =
        Error.NotFound("Order.NotFound", "The order was not found.");

    public static readonly Error AlreadyCancelled =
        Error.Conflict("Order.AlreadyCancelled", "The order has already been cancelled.");

    public static readonly Error CannotCancelConfirmed =
        Error.Conflict("Order.CannotCancelConfirmed", "A confirmed order cannot be cancelled.");

    public static readonly Error EmptyItems =
        Error.Validation("Order.EmptyItems", "An order must contain at least one item.");

    public static readonly Error InvalidQuantity =
        Error.Validation("Order.InvalidQuantity", "Item quantity must be greater than zero.");

    public static readonly Error InvalidPrice =
        Error.Validation("Order.InvalidPrice", "Item unit price must be greater than zero.");
}
