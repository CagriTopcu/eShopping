using Shared.BuildingBlocks.Results;

namespace Catalog.Domain.Errors;

public static class ProductErrors
{
    public static readonly Error NotFound =
        Error.NotFound("Product.NotFound", "Product was not found.");

    public static readonly Error NameRequired =
        Error.Validation("Product.NameRequired", "Product name is required.");

    public static readonly Error NameTooLong =
        Error.Validation("Product.NameTooLong", "Name must not exceed 200 characters.");

    public static readonly Error NegativePrice =
        Error.Validation("Product.NegativePrice", "Price cannot be negative.");

    public static readonly Error InvalidCurrency =
        Error.Validation("Product.InvalidCurrency", "Currency must be a 3-character ISO code.");

    public static readonly Error CategoryRequired =
        Error.Validation("Product.CategoryRequired", "Category is required.");

    public static readonly Error CategoryTooLong =
        Error.Validation("Product.CategoryTooLong", "Category must not exceed 100 characters.");

    public static readonly Error AlreadyDeleted =
        Error.Conflict("Product.AlreadyDeleted", "Product has already been deleted.");
}
