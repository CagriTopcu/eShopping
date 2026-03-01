using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace ServiceDefaults;

public static class AuthExtensions
{
    public const string GatewayHeadersScheme = "GatewayHeaders";

    public static IServiceCollection AddServiceAuthentication(this IServiceCollection services)
    {
        services.AddAuthentication(GatewayHeadersScheme)
            .AddScheme<GatewayAuthenticationOptions, GatewayAuthenticationHandler>(
                GatewayHeadersScheme, _ => { });

        return services;
    }

    public static IServiceCollection AddAuthorizationPolicies(this IServiceCollection services)
    {
        services.AddAuthorizationBuilder()
            .SetDefaultPolicy(new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build())
            .AddPolicy("RequireCustomer", policy => policy.RequireRole("Customer", "Admin"))
            .AddPolicy("RequireAdmin", policy => policy.RequireRole("Admin"));

        return services;
    }

    public static IServiceCollection AddCurrentUser(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUser>();
        return services;
    }
}

public interface ICurrentUser
{
    Guid? UserId { get; }
    bool IsAuthenticated { get; }
    string[] Roles { get; }
    bool IsInRole(string role);
}

internal sealed class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    private ClaimsPrincipal? User => httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

    public Guid? UserId
    {
        get
        {
            var value = User?.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(value, out var id) ? id : null;
        }
    }

    public string[] Roles =>
        User?.FindAll(ClaimTypes.Role).Select(c => c.Value).ToArray() ?? [];

    public bool IsInRole(string role) => User?.IsInRole(role) ?? false;
}
