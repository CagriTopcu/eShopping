using Basket.Application.Mappings;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Shared.BuildingBlocks.Behaviors;

namespace Basket.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        MappingConfig.Configure();

        return services;
    }
}
