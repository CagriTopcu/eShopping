using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace ServiceDefaults.CorrelationId;

/// <summary>
/// Middleware that ensures every request has a correlation ID.
/// If the incoming request contains an X-Correlation-Id header, it is reused;
/// otherwise a new one is generated. The ID is stored in HttpContext.Items
/// and written back to the response header for end-to-end traceability.
/// </summary>
public sealed class CorrelationIdMiddleware(RequestDelegate next)
{
    public const string HeaderName = "X-Correlation-Id";
    public const string ItemKey = "CorrelationId";

    public Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Headers.TryGetValue(HeaderName, out StringValues correlationId)
            || StringValues.IsNullOrEmpty(correlationId))
        {
            correlationId = Guid.NewGuid().ToString("N");
        }

        context.Items[ItemKey] = correlationId.ToString();

        context.Response.OnStarting(() =>
        {
            context.Response.Headers.TryAdd(HeaderName, correlationId);
            return Task.CompletedTask;
        });

        return next(context);
    }
}
