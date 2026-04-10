using Shared.BuildingBlocks.Results;

namespace Basket.Domain.Errors;

public static class BasketErrors
{
    public static readonly Error NotFound =
        Error.NotFound("Basket.NotFound", "Basket was not found.");

    public static readonly Error ItemNotFound =
        Error.NotFound("Basket.ItemNotFound", "Item was not found in basket.");

    public static readonly Error InvalidQuantity =
        Error.Validation("Basket.InvalidQuantity", "Quantity must be greater than zero.");

    public static readonly Error ProductNotFound =
        Error.NotFound("Basket.ProductNotFound", "Product does not exist in the catalog.");

    public static readonly Error OutOfStock =
        Error.Validation("Basket.OutOfStock", "This product is currently out of stock.");

    public static Error InsufficientStock(int available) =>
        Error.Validation("Basket.InsufficientStock",
            $"Requested quantity exceeds available stock ({available} unit(s) remaining).");

    public static readonly Error BasketFull =
        Error.Validation("Basket.BasketFull",
            "Basket cannot exceed 50 distinct products.");

    public static readonly Error CurrencyMismatch =
        Error.Validation("Basket.CurrencyMismatch",
            "All items in a basket must share the same currency.");
}
