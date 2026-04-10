using Catalog.Application.Abstractions;
using Catalog.Infrastructure.HealthChecks;
using Catalog.Infrastructure.Persistence;
using Catalog.Infrastructure.Persistence.Elasticsearch;
using Catalog.Infrastructure.Persistence.Indexes;
using Catalog.Infrastructure.Persistence.Repositories;
using Catalog.Infrastructure.Seeding;
using Elastic.Clients.Elasticsearch;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using ServiceDefaults.CorrelationId;

namespace Catalog.Infrastructure;

public static class DependencyInjection
{
    public static IHostApplicationBuilder AddInfrastructure(this IHostApplicationBuilder builder)
    {
        var configuration = builder.Configuration;

        var mongoConnectionString = configuration.GetConnectionString("catalog-db")
            ?? throw new InvalidOperationException(
                "MongoDB connection string 'catalog-db' is not configured.");

        BsonMappingConfig.Register();

        builder.Services.AddSingleton<IMongoClient>(_ => new MongoClient(mongoConnectionString));
        builder.Services.AddSingleton(sp =>
            sp.GetRequiredService<IMongoClient>().GetDatabase("catalog"));
        builder.Services.AddSingleton<CatalogDbContext>();

        var esConnectionString = configuration.GetConnectionString("elasticsearch")
            ?? throw new InvalidOperationException(
                "Elasticsearch connection string 'elasticsearch' is not configured.");

        builder.Services.AddSingleton<ElasticsearchClient>(_ =>
            new ElasticsearchClient(new Uri(esConnectionString)));

        builder.Services.AddScoped<IProductWriteRepository, ProductWriteRepository>();
        builder.Services.AddScoped<IProductReadRepository, ProductElasticsearchReadRepository>();

        builder.Services.AddHostedService<CatalogElasticsearchInitializer>();
        builder.Services.AddHostedService<CatalogDataSeeder>();

        builder.Services.AddHealthChecks()
            .AddCheck<MongoDbHealthCheck>("mongodb", tags: ["ready"])
            .AddCheck<ElasticsearchHealthCheck>("elasticsearch", tags: ["ready"]);

        // Outbox Strategy: Catalog uses MongoDB which does not support multi-document transactions
        // without a replica set. Therefore, we accept eventual consistency for published events.
        // All downstream consumers (Stock.ProductCreatedConsumer, Basket snapshot consumers) are
        // idempotent to handle duplicate or replayed messages safely.
        // Upgrade path: deploy MongoDB as a replica set to enable transactions, then add a
        // custom MongoDB-based outbox or use MassTransit's MongoDB outbox transport.
        builder.Services.AddMassTransit(x =>
        {
            x.UsingRabbitMq((ctx, cfg) =>
            {
                cfg.Host(configuration.GetConnectionString("rabbitmq"));
                cfg.UseMessageRetry(r => r.Intervals(500, 1000, 2000, 5000));
                cfg.UsePublishFilter(typeof(CorrelationIdPublishFilter<>), ctx);
                cfg.UseConsumeFilter(typeof(CorrelationIdConsumeFilter<>), ctx);
                cfg.ConfigureEndpoints(ctx);
            });
        });

        return builder;
    }

    public static async Task EnsureIndexesAsync(IServiceProvider services)
    {
        var dbContext = services.GetRequiredService<CatalogDbContext>();
        await CatalogIndexes.EnsureIndexesAsync(dbContext);
    }
}
