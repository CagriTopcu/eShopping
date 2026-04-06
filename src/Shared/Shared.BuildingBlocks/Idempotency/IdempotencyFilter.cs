using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace Shared.BuildingBlocks.Idempotency;

/// <summary>
/// Minimal API endpoint filter that enforces idempotency on mutating requests (POST/PUT/PATCH).
/// Clients send an "Idempotency-Key" header; if the same key was already processed,
/// the cached response is returned immediately without re-executing the handler.
/// </summary>
public sealed class IdempotencyFilter(IDistributedCache cache, ILogger<IdempotencyFilter> logger)
    : IEndpointFilter
{
    public const string HeaderName = "Idempotency-Key";
    private static readonly TimeSpan DefaultTtl = TimeSpan.FromHours(24);

    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        var httpContext = context.HttpContext;
        var method = httpContext.Request.Method;

        // Only enforce on mutating methods
        if (!HttpMethods.IsPost(method) && !HttpMethods.IsPut(method) && !HttpMethods.IsPatch(method))
        {
            return await next(context);
        }

        if (!httpContext.Request.Headers.TryGetValue(HeaderName, out var idempotencyKey)
            || string.IsNullOrWhiteSpace(idempotencyKey))
        {
            // No idempotency key provided — proceed normally
            return await next(context);
        }

        var cacheKey = $"idempotency:{idempotencyKey}";

        var existing = await cache.GetStringAsync(cacheKey, httpContext.RequestAborted);
        if (existing is not null)
        {
            logger.LogInformation(
                "Idempotent request detected for key {IdempotencyKey}. Returning cached response.",
                idempotencyKey.ToString());

            httpContext.Response.Headers.Append("X-Idempotent-Replayed", "true");
            return Microsoft.AspNetCore.Http.Results.Content(existing, "application/json", statusCode: 200);
        }

        var result = await next(context);

        // Cache the result for future duplicate requests
        await cache.SetStringAsync(
            cacheKey,
            System.Text.Json.JsonSerializer.Serialize(result),
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = DefaultTtl },
            httpContext.RequestAborted);

        return result;
    }
}
