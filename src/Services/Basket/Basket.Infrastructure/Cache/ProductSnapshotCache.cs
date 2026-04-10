using System.Text.Json;
using Basket.Application.Abstractions;
using Basket.Application.DTOs;
using StackExchange.Redis;

namespace Basket.Infrastructure.Cache;

internal sealed class ProductSnapshotCache(IConnectionMultiplexer redis) : IProductSnapshotCache
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private static readonly TimeSpan Ttl = TimeSpan.FromHours(24);

    private static string Key(Guid productId) => $"product-snapshot:{productId}";

    public async Task<ProductSnapshot?> GetAsync(Guid productId, CancellationToken ct = default)
    {
        var db = redis.GetDatabase();
        var value = await db.StringGetAsync(Key(productId));

        return value.IsNullOrEmpty
            ? null
            : JsonSerializer.Deserialize<ProductSnapshot>((string)value!, JsonOptions);
    }

    public async Task SetAsync(ProductSnapshot snapshot, CancellationToken ct = default)
    {
        var db = redis.GetDatabase();
        var json = JsonSerializer.Serialize(snapshot, JsonOptions);
        await db.StringSetAsync(Key(snapshot.ProductId), json, Ttl);
    }

    public async Task RemoveAsync(Guid productId, CancellationToken ct = default)
    {
        var db = redis.GetDatabase();
        await db.KeyDeleteAsync(Key(productId));
    }
}
