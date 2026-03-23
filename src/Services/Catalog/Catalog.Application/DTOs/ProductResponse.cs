namespace Catalog.Application.DTOs;

/// <summary>Represents a product in the catalog.</summary>
/// <param name="Id">Unique product identifier.</param>
/// <param name="Name">Product display name.</param>
/// <param name="Category">Product category (e.g., Electronics, Clothing).</param>
/// <param name="Price">Unit price in the specified currency.</param>
/// <param name="Currency">ISO 4217 currency code (e.g., USD, EUR, TRY).</param>
/// <param name="Stock">Current available stock quantity.</param>
/// <param name="Description">Optional detailed product description.</param>
/// <param name="ImageUrl">Optional URL to the product image.</param>
/// <param name="CreatedAt">Timestamp when the product was created (UTC).</param>
/// <param name="UpdatedAt">Timestamp of the last update (UTC), null if never updated.</param>
public sealed record ProductResponse(
    Guid Id,
    string Name,
    string Category,
    decimal Price,
    string Currency,
    int Stock,
    string? Description,
    string? ImageUrl,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
