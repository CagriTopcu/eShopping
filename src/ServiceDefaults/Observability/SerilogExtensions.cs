using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace ServiceDefaults.Observability;

public static class SerilogExtensions
{
    /// <summary>
    /// Configures Serilog for structured console output only.
    /// OTLP log export is handled separately by OpenTelemetry Logging provider.
    /// Serilog is added as an additional provider (not replacing ILoggerFactory),
    /// so OpenTelemetry log exporter continues to work.
    /// Enriches every log entry with CorrelationId, UserId, and ServiceName.
    /// </summary>
    public static IHostApplicationBuilder AddStructuredLogging(this IHostApplicationBuilder builder)
    {
        var serviceName = builder.Environment.ApplicationName;

        var logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.AspNetCore.Routing", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Warning)
            .MinimumLevel.Override("System.Net.Http.HttpClient", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithThreadId()
            .Enrich.WithProperty("Application", serviceName)
            .Enrich.WithProperty("ServiceName", serviceName)
            .WriteTo.Console(outputTemplate:
                "[{Timestamp:HH:mm:ss} {Level:u3}] [{ServiceName}] {Message:lj} " +
                "{Properties:j}{NewLine}{Exception}")
            .CreateLogger();

        // Add Serilog as an ILoggerProvider WITHOUT clearing other providers.
        // This preserves the OpenTelemetry log provider for OTLP export to Collector → Loki.
        builder.Logging.AddSerilog(logger, dispose: true);

        // Register the HttpContext enricher so CorrelationId/UserId flow into logs
        // from HTTP request context. For MassTransit consumers, the CorrelationIdConsumeFilter
        // pushes CorrelationId via LogContext.PushProperty instead.
        builder.Services.AddSingleton<ILogEventEnricher>(sp =>
            new HttpContextLogEnricher(
                sp.GetRequiredService<IHttpContextAccessor>(),
                serviceName));

        return builder;
    }
}
