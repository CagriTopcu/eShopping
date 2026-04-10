using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Payment.Application.Abstractions;
using Payment.Application.Consumers;
using Payment.Infrastructure.Gateway;
using Payment.Infrastructure.HealthChecks;
using Payment.Infrastructure.Persistence;
using Payment.Infrastructure.Persistence.Repositories;
using ServiceDefaults.CorrelationId;

namespace Payment.Infrastructure;

public static class DependencyInjection
{
    public static IHostApplicationBuilder AddInfrastructure(this IHostApplicationBuilder builder)
    {
        builder.AddNpgsqlDbContext<PaymentDbContext>("payment-db");

        builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
        builder.Services.AddHostedService<PaymentDbInitializer>();

        builder.Services.Configure<FakePaymentOptions>(
            builder.Configuration.GetSection("FakePayment"));
        builder.Services.AddScoped<IPaymentGateway, FakePaymentGateway>();

        builder.Services.AddHealthChecks()
            .AddCheck<PostgresHealthCheck>("postgresql", tags: ["ready"]);

        builder.Services.AddMassTransit(x =>
        {
            x.AddConsumer<ProcessPaymentConsumer>();

            x.AddEntityFrameworkOutbox<PaymentDbContext>(o =>
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
