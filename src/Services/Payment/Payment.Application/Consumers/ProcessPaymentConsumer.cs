using System.Diagnostics;
using MassTransit;
using Microsoft.Extensions.Logging;
using Payment.Application.Abstractions;
using Payment.Domain.ValueObjects;
using Shared.BuildingBlocks.Observability;
using Shared.Contracts.Commands.Payment;
using Shared.Contracts.Events.Payment;

namespace Payment.Application.Consumers;

public sealed class ProcessPaymentConsumer(
    IPaymentRepository paymentRepository,
    IPaymentGateway paymentGateway,
    BusinessMetrics metrics,
    ILogger<ProcessPaymentConsumer> logger) : IConsumer<ProcessPaymentCommand>
{
    public async Task Consume(ConsumeContext<ProcessPaymentCommand> context)
    {
        using var activity = DiagnosticConfig.Source.StartActivity("payment.process");
        var command = context.Message;
        activity?.SetTag("order.id", command.OrderId.ToString());
        activity?.SetTag("payment.amount", command.Amount);
        activity?.SetTag("payment.currency", command.Currency);

        var stopwatch = Stopwatch.StartNew();

        logger.LogInformation("Processing payment for order {OrderId}, amount {Amount} {Currency}",
            command.OrderId, command.Amount, command.Currency);

        var gatewayResult = await paymentGateway.ProcessAsync(
            command.OrderId, command.Amount, command.Currency, context.CancellationToken);

        var money = Money.Create(command.Amount, command.Currency);
        var card = CardInfo.CreateAnonymous();

        if (!gatewayResult.Success)
        {
            var failedPayment = Payment.Domain.Entities.Payment.CreateFailed(
                command.OrderId, command.CustomerId, money, card,
                gatewayResult.TransactionId, gatewayResult.FailureReason ?? "Payment declined");

            await paymentRepository.AddAsync(failedPayment, context.CancellationToken);
            await paymentRepository.SaveChangesAsync(context.CancellationToken);

            stopwatch.Stop();
            metrics.RecordPaymentDuration(stopwatch.Elapsed.TotalMilliseconds);
            metrics.PaymentFailed(gatewayResult.FailureReason ?? "declined");
            activity?.SetStatus(ActivityStatusCode.Error, gatewayResult.FailureReason ?? "declined");

            logger.LogWarning("Payment failed for order {OrderId}: {Reason}",
                command.OrderId, gatewayResult.FailureReason);

            await context.Publish(new PaymentFailedIntegrationEvent(
                command.OrderId,
                gatewayResult.FailureReason ?? "Payment declined"), context.CancellationToken);
            return;
        }

        var result = Payment.Domain.Entities.Payment.Reserve(
            command.OrderId, command.CustomerId, money, card, gatewayResult.TransactionId);

        if (result.IsFailure)
        {
            stopwatch.Stop();
            metrics.RecordPaymentDuration(stopwatch.Elapsed.TotalMilliseconds);
            metrics.PaymentFailed("domain_error");
            activity?.SetStatus(ActivityStatusCode.Error, result.Error.Description);

            logger.LogError("Payment domain error for order {OrderId}: {Error}",
                command.OrderId, result.Error.Description);

            await context.Publish(new PaymentFailedIntegrationEvent(
                command.OrderId, result.Error.Description), context.CancellationToken);
            return;
        }

        await paymentRepository.AddAsync(result.Value, context.CancellationToken);
        await paymentRepository.SaveChangesAsync(context.CancellationToken);

        stopwatch.Stop();
        metrics.RecordPaymentDuration(stopwatch.Elapsed.TotalMilliseconds);

        logger.LogInformation("Payment reserved for order {OrderId}, transaction {TransactionId}",
            command.OrderId, gatewayResult.TransactionId);

        await context.Publish(new PaymentReservedIntegrationEvent(
            command.OrderId, result.Value.Id.Value, gatewayResult.TransactionId), context.CancellationToken);
    }
}
