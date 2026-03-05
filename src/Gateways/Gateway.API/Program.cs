using System.Security.Claims;
using Gateway.API.Auth;
using Gateway.API.OpenApi;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Scalar.AspNetCore;
using ServiceDefaults;
using Yarp.ReverseProxy.Transforms;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddSingleton<OpenApiAggregator>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var baseUrl = builder.Configuration["services:keycloak:http:0"]
                   ?? builder.Configuration["Keycloak:auth-server-url"]
                   ?? "http://localhost:8080";
        var realm = builder.Configuration["Keycloak:realm"] ?? "eshopping";
        options.Authority = $"{baseUrl}/realms/{realm}";
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters.ValidateAudience = false;
        options.BackchannelHttpHandler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };
    });

builder.Services.AddAuthorizationBuilder()
    .SetDefaultPolicy(new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build())
    .AddPolicy("RequireCustomer", policy => policy.RequireRole("Customer", "Admin"))
    .AddPolicy("RequireAdmin", policy => policy.RequireRole("Admin"));

builder.Services.AddScoped<IClaimsTransformation, KeycloakRolesClaimsTransformation>();

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddServiceDiscoveryDestinationResolver()
    .AddTransforms(ctx =>
    {
        ctx.AddRequestTransform(async transformCtx =>
        {
            transformCtx.ProxyRequest.Headers.Remove("X-User-Id");
            transformCtx.ProxyRequest.Headers.Remove("X-User-Roles");

            var user = transformCtx.HttpContext.User;
            if (user.Identity?.IsAuthenticated == true)
            {
                var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                          ?? user.FindFirst("sub")?.Value;

                if (!string.IsNullOrWhiteSpace(userId))
                {
                    transformCtx.ProxyRequest.Headers
                        .TryAddWithoutValidation("X-User-Id", userId);
                }

                var roles = user.FindAll(ClaimTypes.Role)
                               .Select(c => c.Value)
                               .ToArray();

                if (roles.Length > 0)
                {
                    transformCtx.ProxyRequest.Headers
                        .TryAddWithoutValidation("X-User-Roles", string.Join(",", roles));
                }
            }

            await ValueTask.CompletedTask;
        });
    });

var app = builder.Build();

app.MapGet("/openapi/v1.json", async (
    OpenApiAggregator aggregator,
    bool refresh = false,
    CancellationToken ct = default) =>
{
    var spec = await aggregator.GetAggregatedSpecAsync(refresh, ct);
    return Results.Content(spec, "application/json");
})
.ExcludeFromDescription();

app.MapScalarApiReference(options =>
{
    options.Title = "eShopping API";
    options.OpenApiRoutePattern = "/openapi/v1.json";
});
app.MapDefaultEndpoints();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health", () =>
    TypedResults.Ok(new { Status = "Healthy", Timestamp = DateTime.UtcNow }))
    .WithName("HealthCheck")
    .WithTags("Gateway");

app.MapReverseProxy();

app.Run();
