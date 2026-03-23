namespace Catalog.Application.DTOs;

/// <summary>Request payload for updating an existing product.</summary>
/// <param name="Name">Updated product display name.</param>
/// <param name="Category">Updated product category.</param>
/// <param name="Price">Updated unit price. Must be greater than zero.</param>
/// <param name="Currency">ISO 4217 currency code (e.g., USD, EUR, TRY).</param>
/// <param name="Description">Optional updated product description.</param>
/// <param name="ImageUrl">Optional updated URL to the product image.</param>
public sealed record UpdateProductRequest(
    string Name,
    string Category,
    decimal Price,
    string Currency,
    string? Description,
    string? ImageUrl);
