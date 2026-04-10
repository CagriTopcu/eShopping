using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ServiceDefaults.CorrelationId;
using UserProfile.Application.Abstractions;
using UserProfile.Infrastructure.HealthChecks;
using UserProfile.Infrastructure.Persistence;

namespace UserProfile.Infrastructure;

public static class DependencyInjection
{
    public static IHostApplicationBuilder AddInfrastructure(this IHostApplicationBuilder builder)
    {
        builder.AddNpgsqlDbContext<UserProfileDbContext>("userprofile-db");

        builder.Services.AddScoped<IProfileRepository, ProfileRepository>();

        builder.Services.AddHostedService<UserProfileDbInitializer>();

        builder.Services.AddHealthChecks()
            .AddCheck<PostgresHealthCheck>("postgresql", tags: ["ready"]);

        builder.Services.AddMassTransit(x =>
        {
            x.AddEntityFrameworkOutbox<UserProfileDbContext>(o =>
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
