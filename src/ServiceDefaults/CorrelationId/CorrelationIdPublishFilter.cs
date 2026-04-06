using MassTransit;
using Microsoft.AspNetCore.Http;

namespace ServiceDefaults.CorrelationId;

/// <summary>
/// MassTransit publish/send filter that injects the current correlation ID
/// into message headers, enabling end-to-end tracing across async messaging.
/// </summary>
public sealed class CorrelationIdPublishFilter<T>(
    IHttpContextAccessor httpContextAccessor) : IFilter<PublishContext<T>> where T : class
{
    public Task Send(PublishContext<T> context, IPipe<PublishContext<T>> next)
    {
        if (httpContextAccessor.HttpContext?.Items.TryGetValue(
                CorrelationIdMiddleware.ItemKey, out var correlationId) == true
            && correlationId is string id)
        {
            context.Headers.Set(CorrelationIdMiddleware.HeaderName, id);
        }

        return next.Send(context);
    }

    public void Probe(ProbeContext context) =>
        context.CreateFilterScope("correlationId-publish");
}
