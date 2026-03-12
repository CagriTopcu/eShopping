using System.Text.Json;
using Basket.Application.Abstractions;
using Basket.Domain.Entities;
using Basket.Infrastructure.Persistence.Documents;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Basket.Infrastructure.Persistence;

internal sealed class BasketRedisRepository(
    IConnectionMultiplexer redis,
    IOptions<BasketOptions> options)
    : IBasketRepository
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly TimeSpan _ttl = TimeSpan.FromDays(options.Value.RedisTtlDays);

    private static string Key(string username) => $"basket:{username}";

    public async Task<Basket.Domain.Entities.Basket?> GetAsync(string username, CancellationToken ct = default)
    {
        var db = redis.GetDatabase();
        var value = await db.StringGetAsync(Key(username));

        if (value.IsNullOrEmpty)
            return null;

        var document = JsonSerializer.Deserialize<BasketDocument>((string)value!, JsonOptions);

        if (document is null)
            return null;

        var items = document.Items.Select(i =>
            new BasketItem(i.ProductId, i.ProductName, i.UnitPrice, i.Currency, i.Quantity));

        return Basket.Domain.Entities.Basket.Reconstitute(document.Username, items);
    }

    public async Task SaveAsync(Basket.Domain.Entities.Basket basket, CancellationToken ct = default)
    {
        var document = new BasketDocument
        {
            Username = basket.Id.Value,
            Items = basket.Items.Select(i => new BasketItemDocument
            {
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                UnitPrice = i.UnitPrice,
                Currency = i.Currency,
                Quantity = i.Quantity
            }).ToList()
        };

        var db = redis.GetDatabase();
        var json = JsonSerializer.Serialize(document, JsonOptions);
        await db.StringSetAsync(Key(basket.Id.Value), json, _ttl);
    }

    public async Task DeleteAsync(string username, CancellationToken ct = default)
    {
        var db = redis.GetDatabase();
        await db.KeyDeleteAsync(Key(username));
    }
}
