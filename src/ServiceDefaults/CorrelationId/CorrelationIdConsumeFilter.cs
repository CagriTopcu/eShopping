using MassTransit;
using Serilog.Context;

namespace ServiceDefaults.CorrelationId;

/// <summary>
/// MassTransit consume filter that extracts the correlation ID from incoming
/// message headers and pushes it into the Serilog LogContext.
/// This complements <see cref="CorrelationIdPublishFilter{T}"/> which sets the
/// header on outgoing messages — together they ensure the same correlation ID
/// flows from HTTP request → publish → consume → log entries.
/// </summary>
public sealed class CorrelationIdConsumeFilter<T>(
    ) : IFilter<ConsumeContext<T>> where T : class
{
    public async Task Send(ConsumeContext<T> context, IPipe<ConsumeContext<T>> next)
    {
        var correlationId = context.Headers.Get<string>(CorrelationIdMiddleware.HeaderName)
            ?? context.CorrelationId?.ToString("N")
            ?? Guid.NewGuid().ToString("N");

        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await next.Send(context);
        }
    }

    public void Probe(ProbeContext context) =>
        context.CreateFilterScope("correlationId-consume");
}
