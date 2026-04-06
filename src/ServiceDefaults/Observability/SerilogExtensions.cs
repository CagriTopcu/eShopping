using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

namespace ServiceDefaults.Observability;

public static class SerilogExtensions
{
    /// <summary>
    /// Configures Serilog for structured console output only.
    /// OTLP log export is handled separately by OpenTelemetry Logging provider.
    /// Serilog is added as an additional provider (not replacing ILoggerFactory),
    /// so OpenTelemetry log exporter continues to work.
    /// </summary>
    public static IHostApplicationBuilder AddStructuredLogging(this IHostApplicationBuilder builder)
    {
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
            .Enrich.WithProperty("Application", builder.Environment.ApplicationName)
            .WriteTo.Console(outputTemplate:
                "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} " +
                "{Properties:j}{NewLine}{Exception}")
            .CreateLogger();

        // Add Serilog as an ILoggerProvider WITHOUT clearing other providers.
        // This preserves the OpenTelemetry log provider for OTLP export to Collector → Loki.
        builder.Logging.AddSerilog(logger, dispose: true);

        return builder;
    }
}
