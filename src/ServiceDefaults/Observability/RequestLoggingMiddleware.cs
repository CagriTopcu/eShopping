using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using ServiceDefaults.CorrelationId;

namespace ServiceDefaults.Observability;

/// <summary>
/// Logs every HTTP request with method, path, status code, elapsed time,
/// and correlation ID. Provides structured logging for production diagnostics.
/// </summary>
public sealed class RequestLoggingMiddleware(
    RequestDelegate next,
    ILogger<RequestLoggingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Items.TryGetValue(
            CorrelationIdMiddleware.ItemKey, out var cid) ? cid?.ToString() : null;

        var stopwatch = Stopwatch.StartNew();

        try
        {
            await next(context);
            stopwatch.Stop();

            var statusCode = context.Response.StatusCode;
            var level = statusCode >= 500 ? LogLevel.Error
                : statusCode >= 400 ? LogLevel.Warning
                : LogLevel.Information;

            logger.Log(level,
                "HTTP {Method} {Path} responded {StatusCode} in {ElapsedMs}ms [CorrelationId={CorrelationId}]",
                context.Request.Method,
                context.Request.Path.Value,
                statusCode,
                stopwatch.ElapsedMilliseconds,
                correlationId);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            stopwatch.Stop();

            logger.LogError(ex,
                "HTTP {Method} {Path} threw exception after {ElapsedMs}ms [CorrelationId={CorrelationId}]",
                context.Request.Method,
                context.Request.Path.Value,
                stopwatch.ElapsedMilliseconds,
                correlationId);

            throw;
        }
    }
}
