using System.Text.Json;
using Asp.Versioning;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using ServiceDefaults.CorrelationId;
using ServiceDefaults.Observability;

namespace ServiceDefaults;

public static class Extensions
{
    public static IHostApplicationBuilder AddServiceDefaults(this IHostApplicationBuilder builder)
    {
        builder.ConfigureOpenTelemetry();
        builder.AddStructuredLogging();
        builder.AddDefaultHealthChecks();

        // Ensure OpenTelemetry log provider receives Information+ logs
        // even when Serilog filters are more restrictive on its own pipeline
        builder.Logging.AddFilter<OpenTelemetry.Logs.OpenTelemetryLoggerProvider>(
            null, LogLevel.Information);

        builder.Services.AddServiceDiscovery();
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddTransient<CorrelationIdDelegatingHandler>();

        builder.Services.ConfigureHttpClientDefaults(http =>
        {
            http.AddHttpMessageHandler<CorrelationIdDelegatingHandler>();
            http.AddStandardResilienceHandler();
            http.AddServiceDiscovery();
        });

        return builder;
    }

    public static IHostApplicationBuilder ConfigureOpenTelemetry(this IHostApplicationBuilder builder)
    {
        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;

            // Export logs via OTLP HTTP directly to Loki
            var lokiEndpoint = Environment.GetEnvironmentVariable("LOKI_OTLP_ENDPOINT")
                ?? builder.Configuration["LOKI_OTLP_ENDPOINT"];
            if (!string.IsNullOrWhiteSpace(lokiEndpoint))
            {
                logging.AddOtlpExporter(o =>
                {
                    o.Endpoint = new Uri(lokiEndpoint);
                    o.Protocol = OtlpExportProtocol.HttpProtobuf;
                });
            }
        });

        builder.Services.AddOpenTelemetry()
            .WithMetrics(metrics =>
            {
                metrics
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddMeter("MassTransit");
            })
            .WithTracing(tracing =>
            {
                tracing
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddSource("MassTransit")
                    .AddSource("Microsoft.EntityFrameworkCore");
            });

        builder.AddOpenTelemetryExporters();

        return builder;
    }

    private static IHostApplicationBuilder AddOpenTelemetryExporters(this IHostApplicationBuilder builder)
    {
        // Aspire Dashboard OTLP endpoint
        var otlpEndpoint = builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"];
        if (!string.IsNullOrWhiteSpace(otlpEndpoint))
        {
            builder.Services.AddOpenTelemetry()
                .WithTracing(tracing => tracing.AddOtlpExporter("aspire", o =>
                    o.Endpoint = new Uri(otlpEndpoint)))
                .WithMetrics(metrics => metrics.AddOtlpExporter("aspire", o =>
                    o.Endpoint = new Uri(otlpEndpoint)));
        }

        // OTel Collector → Grafana stack (Prometheus, Tempo, Loki)
        var collectorEndpoint = builder.Configuration["OTEL_COLLECTOR_ENDPOINT"];
        if (!string.IsNullOrWhiteSpace(collectorEndpoint))
        {
            builder.Services.AddOpenTelemetry()
                .WithTracing(tracing => tracing.AddOtlpExporter("collector", o =>
                {
                    o.Endpoint = new Uri(collectorEndpoint);
                    o.Protocol = OtlpExportProtocol.Grpc;
                }))
                .WithMetrics(metrics => metrics.AddOtlpExporter("collector", o =>
                {
                    o.Endpoint = new Uri(collectorEndpoint);
                    o.Protocol = OtlpExportProtocol.Grpc;
                }));
        }

        return builder;
    }

    public static IHostApplicationBuilder AddDefaultHealthChecks(this IHostApplicationBuilder builder)
    {
        builder.Services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);

        return builder;
    }

    public static IServiceCollection AddServiceOpenApi(
        this IServiceCollection services,
        string title,
        string description)
    {
        services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer((document, context, ct) =>
            {
                document.Info = new OpenApiInfo
                {
                    Title = title,
                    Version = "v1",
                    Description = description,
                    Contact = new OpenApiContact
                    {
                        Name = "eShopping Team",
                        Email = "api@eshopping.dev"
                    },
                    License = new OpenApiLicense { Name = "MIT" }
                };

                document.Components ??= new OpenApiComponents();
                document.Components.SecuritySchemes ??=
                    new Dictionary<string, IOpenApiSecurityScheme>();

                document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Description = "JWT token obtained from Keycloak. Format: Bearer {token}"
                };

                return Task.CompletedTask;
            });
        });

        return services;
    }

    public static IServiceCollection AddServiceApiVersioning(this IServiceCollection services)
    {
        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
            options.ApiVersionReader = ApiVersionReader.Combine(
                new UrlSegmentApiVersionReader(),
                new HeaderApiVersionReader("api-version")
            );
        });

        return services;
    }

    public static WebApplication MapDefaultEndpoints(this WebApplication app)
    {
        app.UseMiddleware<CorrelationIdMiddleware>();

        app.UseMiddleware<RequestLoggingMiddleware>();

        app.UseExceptionHandler(errorApp =>
        {
            errorApp.Run(async context =>
            {
                var logger = context.RequestServices.GetRequiredService<ILoggerFactory>()
                    .CreateLogger("GlobalExceptionHandler");

                var exceptionFeature = context.Features
                    .Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();

                if (exceptionFeature?.Error is { } error)
                {
                    logger.LogError(error,
                        "Unhandled exception on {Method} {Path}",
                        context.Request.Method, context.Request.Path);
                }

                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                context.Response.ContentType = "application/problem+json";

                var correlationId = context.Items.TryGetValue(
                    CorrelationIdMiddleware.ItemKey, out var cid) ? cid?.ToString() : null;

                var problem = new
                {
                    type = "https://tools.ietf.org/html/rfc9110#section-15.6.1",
                    title = "An unexpected error occurred",
                    status = 500,
                    traceId = context.TraceIdentifier,
                    correlationId
                };

                await context.Response.WriteAsync(
                    JsonSerializer.Serialize(problem, JsonSerializerOptions.Web));
            });
        });

        app.MapHealthChecks("/health", new HealthCheckOptions
        {
            ResponseWriter = WriteHealthCheckResponse
        });

        app.MapHealthChecks("/alive", new HealthCheckOptions
        {
            Predicate = r => r.Tags.Contains("live")
        });

        return app;
    }

    private static async Task WriteHealthCheckResponse(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json";

        var response = new
        {
            status = report.Status.ToString(),
            totalDuration = report.TotalDuration.TotalMilliseconds,
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                duration = e.Value.Duration.TotalMilliseconds,
                description = e.Value.Description,
                exception = e.Value.Exception?.Message
            })
        };

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(response, JsonSerializerOptions.Web));
    }
}
