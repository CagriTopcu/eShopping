using Basket.Application.Abstractions;
using Basket.Infrastructure.Cache;
using Basket.Infrastructure.Consumers;
using Basket.Infrastructure.HealthChecks;
using Basket.Infrastructure.Persistence;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ServiceDefaults.CorrelationId;

namespace Basket.Infrastructure;

public static class DependencyInjection
{
    public static IHostApplicationBuilder AddInfrastructure(this IHostApplicationBuilder builder)
    {
        builder.AddRedisClient("redis");

        builder.Services.Configure<BasketOptions>(
            builder.Configuration.GetSection(BasketOptions.SectionName));

        builder.Services.AddSingleton<IProductSnapshotCache, ProductSnapshotCache>();

        builder.Services.AddScoped<IBasketRepository, BasketRedisRepository>();

        // HTTP client for one-time cache warmup at startup (fetches existing products from Catalog).
        // After warmup, integration events keep the cache in sync — no ongoing HTTP calls.
        builder.Services.AddHttpClient("CatalogApi", client =>
        {
            client.BaseAddress = new Uri("http://catalog-api");
            client.Timeout = TimeSpan.FromSeconds(30);
        });
        builder.Services.AddHostedService<ProductSnapshotWarmupService>();

        builder.Services.AddHealthChecks()
            .AddCheck<RedisHealthCheck>("redis", tags: ["ready"]);

        // Outbox Strategy: Basket uses Redis (no transactional outbox support).
        // Currently Basket is consumer-only — it does NOT publish any integration events.
        // All consumers are naturally idempotent (Redis SET operations, Delete is idempotent).
        // If Basket needs to publish events in the future, use a sidecar PostgreSQL database
        // for the MassTransit EF outbox, or implement a Redis Streams-based outbox.
        builder.Services.AddMassTransit(x =>
        {
            x.AddConsumer<ClearBasketOnOrderPlacedConsumer>();
            x.AddConsumer<ProductCreatedSnapshotConsumer>();
            x.AddConsumer<ProductUpdatedSnapshotConsumer>();
            x.AddConsumer<ProductPriceChangedSnapshotConsumer>();
            x.AddConsumer<ProductDeletedSnapshotConsumer>();

            x.UsingRabbitMq((ctx, cfg) =>
            {
                cfg.Host(builder.Configuration.GetConnectionString("rabbitmq"));
                cfg.UseMessageRetry(r => r.Intervals(500, 1000, 2000, 5000));
                cfg.UsePublishFilter(typeof(CorrelationIdPublishFilter<>), ctx);
                cfg.UseConsumeFilter(typeof(CorrelationIdConsumeFilter<>), ctx);
                cfg.ConfigureEndpoints(ctx);
            });
        });

        return builder;
    }
}
