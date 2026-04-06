using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Shared.BuildingBlocks.Idempotency;

public static class IdempotencyExtensions
{
    /// <summary>
    /// Adds the idempotency endpoint filter to the route.
    /// Requires IDistributedCache to be registered (e.g. Redis or in-memory).
    /// Usage: app.MapPost("/orders", handler).WithIdempotency();
    /// </summary>
    public static RouteHandlerBuilder WithIdempotency(this RouteHandlerBuilder builder) =>
        builder.AddEndpointFilter<IdempotencyFilter>();

    /// <summary>
    /// Registers a distributed in-memory cache for idempotency if no IDistributedCache
    /// is already registered. Services using Redis should register Redis cache instead.
    /// </summary>
    public static IServiceCollection AddIdempotencySupport(this IServiceCollection services)
    {
        services.AddDistributedMemoryCache();
        return services;
    }
}
