using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Payment.Application.Abstractions;
using Payment.Infrastructure.Gateway;
using Payment.Infrastructure.Persistence;
using Payment.Infrastructure.Persistence.Repositories;

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

        builder.Services.AddMassTransit(x =>
        {
            x.AddEntityFrameworkOutbox<PaymentDbContext>(o =>
            {
                o.UsePostgres();
                o.UseBusOutbox();
            });

            x.UsingRabbitMq((ctx, cfg) =>
            {
                cfg.Host(builder.Configuration.GetConnectionString("rabbitmq"));
                cfg.ConfigureEndpoints(ctx);
            });
        });

        return builder;
    }
}
