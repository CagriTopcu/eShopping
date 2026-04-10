using MassTransit;
using Microsoft.Extensions.Logging;
using Notification.Worker.Domain;
using Notification.Worker.Services;
using Shared.BuildingBlocks.Observability;
using Shared.Contracts.Events.Payment;

namespace Notification.Worker.Consumers;

public sealed class PaymentFailedNotificationConsumer(
    IEmailSender emailSender,
    BusinessMetrics metrics,
    ILogger<PaymentFailedNotificationConsumer> logger) : IConsumer<PaymentFailedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<PaymentFailedIntegrationEvent> context)
    {
        var message = context.Message;

        logger.LogInformation(
            "Processing payment failed notification for order {OrderId}",
            message.OrderId);

        var notification = NotificationRecord.Create(
            NotificationType.PaymentFailed,
            NotificationChannel.Email,
            recipient: "admin@eshopping.local",
            subject: $"Payment Failed - Order #{message.OrderId.ToString()[..8]}",
            body: $"Payment failed for order #{message.OrderId.ToString()[..8]}. Reason: {message.Reason}",
            correlationId: message.OrderId);

        try
        {
            await emailSender.SendAsync(
                notification.Recipient,
                notification.Subject,
                notification.Body,
                context.CancellationToken);

            notification.MarkSent();
            metrics.NotificationSent("payment_failed", "email");

            logger.LogInformation("Payment failed notification sent for order {OrderId}", message.OrderId);
        }
        catch (Exception ex)
        {
            notification.MarkFailed(ex.Message);

            logger.LogError(ex, "Failed to send payment failed notification for order {OrderId}", message.OrderId);

            throw;
        }
    }
}
