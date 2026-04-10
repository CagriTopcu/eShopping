using Microsoft.AspNetCore.Http;
using Serilog.Core;
using Serilog.Events;
using ServiceDefaults.CorrelationId;

namespace ServiceDefaults.Observability;

/// <summary>
/// Serilog enricher that adds CorrelationId, UserId, and ServiceName
/// to every log entry from HttpContext. Works in tandem with
/// <see cref="CorrelationIdMiddleware"/> which stores the correlation ID
/// in HttpContext.Items.
/// </summary>
public sealed class HttpContextLogEnricher(
    IHttpContextAccessor httpContextAccessor,
    string serviceName) : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var httpContext = httpContextAccessor.HttpContext;

        // CorrelationId: from middleware-stored HttpContext.Items
        if (httpContext?.Items.TryGetValue(CorrelationIdMiddleware.ItemKey, out var correlationId) == true
            && correlationId is string cid)
        {
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("CorrelationId", cid));
        }

        // UserId: from authenticated user's "sub" claim (Keycloak standard)
        var userId = httpContext?.User?.FindFirst("sub")?.Value
            ?? httpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(userId))
        {
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("UserId", userId));
        }

        // ServiceName: always present
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("ServiceName", serviceName));
    }
}
