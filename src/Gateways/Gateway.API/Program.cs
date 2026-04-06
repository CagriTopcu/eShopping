using System.Security.Claims;
using System.Threading.RateLimiting;
using Gateway.API.Auth;
using Gateway.API.OpenApi;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Http.Resilience;
using Scalar.AspNetCore;
using ServiceDefaults;
using ServiceDefaults.CorrelationId;
using Yarp.ReverseProxy.Transforms;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddHttpClient(OpenApiAggregator.HttpClientName)
    .ConfigureHttpClient(client => client.Timeout = TimeSpan.FromSeconds(60));

builder.Services.Configure<HttpStandardResilienceOptions>(
    OpenApiAggregator.HttpClientName,
    options =>
    {
        options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(30);
        options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(60);
        options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(120);
    });

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
        if (builder.Environment.IsDevelopment())
        {
            options.BackchannelHttpHandler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback =
                    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };
        }
    });

builder.Services.AddAuthorizationBuilder()
    .SetDefaultPolicy(new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build())
    .AddPolicy("RequireCustomer", policy => policy.RequireRole("Customer", "Admin"))
    .AddPolicy("RequireAdmin", policy => policy.RequireRole("Admin"));

builder.Services.AddScoped<IClaimsTransformation, KeycloakRolesClaimsTransformation>();

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // Per-user policy: authenticated users get their own bucket based on user ID
    options.AddPolicy("per-user", context =>
    {
        var userId = context.User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous";
        return RateLimitPartition.GetTokenBucketLimiter(userId, _ => new TokenBucketRateLimiterOptions
        {
            TokenLimit = 100,
            ReplenishmentPeriod = TimeSpan.FromMinutes(1),
            TokensPerPeriod = 50,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = 10,
            AutoReplenishment = true
        });
    });

    // Global fixed window — protects against unauthenticated floods
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
    {
        return RateLimitPartition.GetFixedWindowLimiter("global", _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 1000,
            Window = TimeSpan.FromMinutes(1),
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = 50
        });
    });
});

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddServiceDiscoveryDestinationResolver()
    .AddTransforms(ctx =>
    {
        ctx.AddRequestTransform(async transformCtx =>
        {
            // Propagate correlation ID to downstream services
            if (transformCtx.HttpContext.Items.TryGetValue(
                    CorrelationIdMiddleware.ItemKey, out var correlationId)
                && correlationId is string cid)
            {
                transformCtx.ProxyRequest.Headers.Remove(CorrelationIdMiddleware.HeaderName);
                transformCtx.ProxyRequest.Headers
                    .TryAddWithoutValidation(CorrelationIdMiddleware.HeaderName, cid);
            }

            transformCtx.ProxyRequest.Headers.Remove("X-User-Id");
            transformCtx.ProxyRequest.Headers.Remove("X-User-Name");
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

                var username = user.FindFirst("preferred_username")?.Value;
                if (!string.IsNullOrWhiteSpace(username))
                {
                    transformCtx.ProxyRequest.Headers
                        .TryAddWithoutValidation("X-User-Name", username);
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

app.MapGet("/openapi/{version}.json", async (
    string version,
    OpenApiAggregator aggregator,
    bool refresh = false,
    CancellationToken ct = default) =>
{
    var spec = await aggregator.GetAggregatedSpecAsync(version, refresh, ct);
    return Results.Content(spec, "application/json");
})
.ExcludeFromDescription();

app.MapScalarApiReference(options =>
{
    options.Title = "eShopping API Gateway";
    options.Theme = ScalarTheme.BluePlanet;
    options.DefaultHttpClient = new(ScalarTarget.CSharp, ScalarClient.HttpClient);
    options.OpenApiRoutePattern = "/openapi/{documentName}.json";
    options.AddPreferredSecuritySchemes("Bearer");
});
app.MapDefaultEndpoints();

app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

app.MapReverseProxy();

app.Run();
