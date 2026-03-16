using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Stock.Application.Abstractions;
using Stock.Infrastructure.Persistence;

namespace Stock.Infrastructure;

public static class DependencyInjection
{
    public static IHostApplicationBuilder AddInfrastructure(this IHostApplicationBuilder builder)
    {
        builder.AddNpgsqlDbContext<StockDbContext>("stock-db");

        builder.Services.AddScoped<IStockRepository, StockRepository>();
        builder.Services.AddHostedService<StockDbInitializer>();

        return builder;
    }
}
