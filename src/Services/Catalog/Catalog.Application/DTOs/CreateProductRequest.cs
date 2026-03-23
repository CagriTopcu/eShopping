namespace Catalog.Application.DTOs;

/// <summary>Request payload for creating a new product.</summary>
/// <param name="Name">Product display name. Must be non-empty.</param>
/// <param name="Category">Product category (e.g., Electronics, Clothing).</param>
/// <param name="Price">Unit price. Must be greater than zero.</param>
/// <param name="Currency">ISO 4217 currency code (e.g., USD, EUR, TRY).</param>
/// <param name="Stock">Initial stock quantity. Must be zero or positive.</param>
/// <param name="Description">Optional detailed product description.</param>
/// <param name="ImageUrl">Optional URL to the product image.</param>
public sealed record CreateProductRequest(
    string Name,
    string Category,
    decimal Price,
    string Currency,
    int Stock,
    string? Description,
    string? ImageUrl);
