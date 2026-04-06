using Microsoft.AspNetCore.Http;

namespace ServiceDefaults.CorrelationId;

/// <summary>
/// Automatically propagates the correlation ID from the current HTTP context
/// to outgoing HttpClient requests. Registered via ConfigureHttpClientDefaults
/// so every service-to-service HTTP call carries the correlation ID.
/// </summary>
public sealed class CorrelationIdDelegatingHandler(
    IHttpContextAccessor httpContextAccessor) : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        if (httpContextAccessor.HttpContext?.Items.TryGetValue(
                CorrelationIdMiddleware.ItemKey, out var correlationId) == true
            && correlationId is string id)
        {
            request.Headers.TryAddWithoutValidation(
                CorrelationIdMiddleware.HeaderName, id);
        }

        return base.SendAsync(request, cancellationToken);
    }
}
