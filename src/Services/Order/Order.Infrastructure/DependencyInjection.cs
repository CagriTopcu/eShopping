using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Order.Application.Abstractions;
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

        return builder;
    }
}
