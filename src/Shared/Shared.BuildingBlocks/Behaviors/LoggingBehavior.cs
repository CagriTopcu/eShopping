using System.Diagnostics;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Shared.BuildingBlocks.Results;

namespace Shared.BuildingBlocks.Behaviors;

public sealed class LoggingBehavior<TRequest, TResponse>(
    ILogger<LoggingBehavior<TRequest, TResponse>> logger,
    IHttpContextAccessor httpContextAccessor)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : Result
{
    private const long SlowRequestThresholdMs = 500;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var correlationId = httpContextAccessor.HttpContext?.Items
            .TryGetValue("CorrelationId", out var cid) == true ? cid?.ToString() : null;

        logger.LogInformation(
            "Handling {RequestName} [CorrelationId={CorrelationId}]",
            requestName, correlationId);

        var stopwatch = Stopwatch.StartNew();
        var response = await next(cancellationToken);
        stopwatch.Stop();

        var elapsedMs = stopwatch.ElapsedMilliseconds;

        if (response.IsSuccess)
        {
            logger.LogInformation(
                "Handled {RequestName} successfully in {ElapsedMs}ms [CorrelationId={CorrelationId}]",
                requestName, elapsedMs, correlationId);
        }
        else
        {
            logger.LogWarning(
                "Request {RequestName} failed in {ElapsedMs}ms: [{ErrorCode}] {ErrorDescription} [CorrelationId={CorrelationId}]",
                requestName, elapsedMs, response.Error.Code, response.Error.Description, correlationId);
        }

        if (elapsedMs > SlowRequestThresholdMs)
        {
            logger.LogWarning(
                "SLOW REQUEST: {RequestName} took {ElapsedMs}ms (threshold: {ThresholdMs}ms) [CorrelationId={CorrelationId}]",
                requestName, elapsedMs, SlowRequestThresholdMs, correlationId);
        }

        return response;
    }
}
