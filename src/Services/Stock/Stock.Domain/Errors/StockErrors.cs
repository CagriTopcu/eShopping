using Shared.BuildingBlocks.Results;

namespace Stock.Domain.Errors;

public static class StockErrors
{
    public static readonly Error NotFound =
        Error.NotFound("Stock.NotFound", "Stock record was not found for this product.");

    public static readonly Error InvalidQuantity =
        Error.Validation("Stock.InvalidQuantity", "Available quantity cannot be negative.");

    public static Error InsufficientStock(int available) =>
        Error.Validation("Stock.InsufficientStock",
            $"Requested quantity exceeds available stock ({available} unit(s) remaining).");
}
