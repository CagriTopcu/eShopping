using System.Text.Json;
using Basket.Application.Abstractions;
using Basket.Application.DTOs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Basket.Infrastructure.Cache;

/// <summary>
/// Pre-populates the product snapshot Redis cache from Catalog API at startup.
/// After warmup, integration events keep the cache in sync.
/// </summary>
internal sealed class ProductSnapshotWarmupService(
    IServiceProvider serviceProvider,
    ILogger<ProductSnapshotWarmupService> logger) : BackgroundService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Small delay to let services start up
        await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);

        for (var attempt = 1; attempt <= 5; attempt++)
        {
            try
            {
                await WarmupCacheAsync(stoppingToken);
                return;
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested && attempt < 5)
            {
                var delay = TimeSpan.FromSeconds(attempt * 3);
                logger.LogWarning(ex,
                    "Product snapshot warmup attempt {Attempt}/5 failed. Retrying in {Delay}s.",
                    attempt, delay.TotalSeconds);
                await Task.Delay(delay, stoppingToken);
            }
        }

        logger.LogError("Product snapshot warmup failed after 5 attempts. Cache will be populated by events.");
    }

    private async Task WarmupCacheAsync(CancellationToken ct)
    {
        using var scope = serviceProvider.CreateScope();
        var cache = scope.ServiceProvider.GetRequiredService<IProductSnapshotCache>();
        var httpClientFactory = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>();

        var client = httpClientFactory.CreateClient("CatalogApi");

        // Fetch first page to get total count, then fetch all
        var firstPage = await FetchPageAsync(client, 1, 100, ct);
        if (firstPage is null || firstPage.Items.Count == 0)
        {
            logger.LogInformation("No products found in catalog — snapshot cache is empty.");
            return;
        }

        var cached = 0;
        foreach (var product in firstPage.Items)
        {
            await cache.SetAsync(ToSnapshot(product), ct);
            cached++;
        }

        // Fetch remaining pages
        for (var page = 2; page <= firstPage.TotalPages; page++)
        {
            var pageResult = await FetchPageAsync(client, page, 100, ct);
            if (pageResult?.Items is null) break;

            foreach (var product in pageResult.Items)
            {
                await cache.SetAsync(ToSnapshot(product), ct);
                cached++;
            }
        }

        logger.LogInformation("Product snapshot cache warmed up with {Count} products.", cached);
    }

    private async Task<CatalogPagedResponse?> FetchPageAsync(
        HttpClient client, int page, int pageSize, CancellationToken ct)
    {
        var response = await client.GetAsync(
            $"/api/v1/catalog/products?page={page}&pageSize={pageSize}", ct);

        response.EnsureSuccessStatusCode();

        return await JsonSerializer.DeserializeAsync<CatalogPagedResponse>(
            await response.Content.ReadAsStreamAsync(ct), JsonOptions, ct);
    }

    private static ProductSnapshot ToSnapshot(CatalogProduct p) =>
        new(p.Id, p.Name, p.Price, p.Currency);

    private sealed record CatalogPagedResponse(
        List<CatalogProduct> Items,
        int TotalPages);

    private sealed record CatalogProduct(
        Guid Id,
        string Name,
        decimal Price,
        string Currency);
}
