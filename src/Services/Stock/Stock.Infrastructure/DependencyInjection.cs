using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Stock.Application.Abstractions;
using Stock.Application.Consumers;
using Stock.Infrastructure.Persistence;
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

        builder.Services.AddMassTransit(x =>
        {
            x.AddConsumer<ReserveStockConsumer>();
            x.AddConsumer<ReleaseStockConsumer>();
            x.AddConsumer<ProductCreatedConsumer>();

            x.UsingRabbitMq((ctx, cfg) =>
            {
                cfg.Host(builder.Configuration.GetConnectionString("rabbitmq"));
                cfg.ConfigureEndpoints(ctx);
            });
        });

        return builder;
    }
}
