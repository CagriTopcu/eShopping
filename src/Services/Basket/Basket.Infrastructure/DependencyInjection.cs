using Basket.Application.Abstractions;
using Basket.Infrastructure.Consumers;
using Basket.Infrastructure.Persistence;
using Basket.Infrastructure.Rest;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Basket.Infrastructure;

public static class DependencyInjection
{
    public static IHostApplicationBuilder AddInfrastructure(this IHostApplicationBuilder builder)
    {
        builder.AddRedisClient("redis");

        builder.Services.Configure<BasketOptions>(
            builder.Configuration.GetSection(BasketOptions.SectionName));

        builder.Services.AddHttpClient<ICatalogClient, CatalogRestClient>(client =>
            client.BaseAddress = new Uri("http://catalog-api"));

        builder.Services.AddHttpClient<IStockClient, StockRestClient>(client =>
            client.BaseAddress = new Uri("http://stock-api"));

        builder.Services.AddScoped<IBasketRepository, BasketRedisRepository>();

        builder.Services.AddMassTransit(x =>
        {
            x.AddConsumer<ClearBasketOnOrderPlacedConsumer>();

            x.UsingRabbitMq((ctx, cfg) =>
            {
                cfg.Host(builder.Configuration.GetConnectionString("rabbitmq"));
                cfg.ConfigureEndpoints(ctx);
            });
        });

        return builder;
    }
}
