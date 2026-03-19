using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Order.Application.Abstractions;
using Order.Application.Sagas;
using Order.Application.Sagas.Activities;
using Order.Infrastructure.Messaging;
using Order.Infrastructure.Persistence;
using Order.Infrastructure.Persistence.Repositories;

namespace Order.Infrastructure;

public static class DependencyInjection
{
    public static IHostApplicationBuilder AddInfrastructure(this IHostApplicationBuilder builder)
    {
        builder.AddNpgsqlDbContext<OrderDbContext>("order-db");

        builder.Services.AddScoped<IOrderRepository, OrderRepository>();
        builder.Services.AddHostedService<OrderDbInitializer>();

        builder.Services.AddHttpClient<IPaymentClient, PaymentClient>(client =>
        {
            client.BaseAddress = new Uri("http://payment-api");
        });

        builder.Services.AddScoped<ProcessPaymentActivity>();
        builder.Services.AddScoped<CancelOrderActivity>();

        builder.Services.AddMassTransit(x =>
        {
            x.AddSagaStateMachine<OrderSaga, OrderSagaState>()
                .EntityFrameworkRepository(r =>
                {
                    r.ConcurrencyMode = ConcurrencyMode.Optimistic;
                    r.ExistingDbContext<OrderDbContext>();
                    r.UsePostgres();
                });

            x.AddEntityFrameworkOutbox<OrderDbContext>(o =>
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
