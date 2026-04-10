using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ServiceDefaults.CorrelationId;
using Shipping.Application.Abstractions;
using Shipping.Application.Consumers;
using Shipping.Infrastructure.HealthChecks;
using Shipping.Infrastructure.Persistence;

namespace Shipping.Infrastructure;

public static class DependencyInjection
{
    public static IHostApplicationBuilder AddInfrastructure(this IHostApplicationBuilder builder)
    {
        builder.AddNpgsqlDbContext<ShippingDbContext>("shipping-db");

        builder.Services.AddScoped<IShipmentRepository, ShipmentRepository>();

        builder.Services.AddHostedService<ShippingDbInitializer>();

        builder.Services.AddHealthChecks()
            .AddCheck<PostgresHealthCheck>("postgresql", tags: ["ready"]);

        builder.Services.AddMassTransit(x =>
        {
            x.AddConsumer<OrderConfirmedConsumer>();

            x.AddEntityFrameworkOutbox<ShippingDbContext>(o =>
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
