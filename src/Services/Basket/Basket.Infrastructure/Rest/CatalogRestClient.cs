using System.Net;
using System.Net.Http.Json;
using Basket.Application.Abstractions;
using Basket.Application.DTOs;

namespace Basket.Infrastructure.Rest;

internal sealed class CatalogRestClient(HttpClient httpClient) : ICatalogClient
{
    public async Task<ProductSnapshot?> GetProductSnapshotAsync(Guid productId, CancellationToken ct = default)
    {
        HttpResponseMessage response;

        try
        {
            response = await httpClient.GetAsync($"api/v1/catalog/products/{productId}", ct);
        }
        catch (HttpRequestException ex)
        {
            throw new CatalogServiceUnavailableException("Catalog REST service is unreachable.", ex);
        }

        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();

        var product = await response.Content.ReadFromJsonAsync<CatalogProductResponse>(cancellationToken: ct);
        if (product is null)
            return null;

        return new ProductSnapshot(
            product.Id,
            product.Name,
            product.Price,
            product.Currency,
            product.Stock);
    }
}

internal sealed record CatalogProductResponse(
    Guid Id,
    string Name,
    string Category,
    decimal Price,
    string Currency,
    int Stock,
    string? Description,
    string? ImageUrl);
