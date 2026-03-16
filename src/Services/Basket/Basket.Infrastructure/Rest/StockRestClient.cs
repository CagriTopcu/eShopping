using System.Net;
using System.Net.Http.Json;
using Basket.Application.Abstractions;
using Basket.Application.DTOs;

namespace Basket.Infrastructure.Rest;

internal sealed class StockRestClient(HttpClient httpClient) : IStockClient
{
    public async Task<StockInfo?> GetStockAsync(Guid productId, CancellationToken ct = default)
    {
        HttpResponseMessage response;

        try
        {
            response = await httpClient.GetAsync($"api/v1/stock/{productId}", ct);
        }
        catch (HttpRequestException ex)
        {
            throw new StockServiceUnavailableException("Stock REST service is unreachable.", ex);
        }

        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<StockInfo>(cancellationToken: ct);
    }
}
