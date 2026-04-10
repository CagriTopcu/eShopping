using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Stock.Application.Abstractions;
using Stock.Application.Consumers;
using Stock.Infrastructure.HealthChecks;
using Stock.Infrastructure.Persistence;
using ServiceDefaults.CorrelationId;
using Stock.Infrastructure.Seeding;

namespace Stock.Infrastructure;

public static class DependencyInjection
{
    public static IHostApplicationBuilder AddInfrastructure(this IHostApplicationBuilder builder)
    {
        builder.AddNpgsqlDbContext<StockDbContext>("stock-db");

        builder.Services.AddScoped<IStockRepository, StockRepository>();
        builder.Services.AddHostedService<StockDbInitializer>();
        builder.Services.AddHostedService<StockDataSeeder>();

        builder.Services.AddHealthChecks()
            .AddCheck<PostgresHealthCheck>("postgresql", tags: ["ready"]);

        builder.Services.AddMassTransit(x =>
        {
            x.AddConsumer<ReserveStockConsumer>();
            x.AddConsumer<ReleaseStockConsumer>();
            x.AddConsumer<ProductCreatedConsumer>();

            x.AddEntityFrameworkOutbox<StockDbContext>(o =>
            {
                o.UsePostgres();
                o.UseBusOutbox();
            });

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
