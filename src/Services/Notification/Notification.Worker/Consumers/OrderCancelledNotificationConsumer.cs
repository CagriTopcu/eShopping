using MassTransit;
using Microsoft.Extensions.Logging;
using Notification.Worker.Domain;
using Notification.Worker.Services;
using Shared.BuildingBlocks.Observability;
using Shared.Contracts.Events.Orders;

namespace Notification.Worker.Consumers;

public sealed class OrderCancelledNotificationConsumer(
    IEmailSender emailSender,
    BusinessMetrics metrics,
    ILogger<OrderCancelledNotificationConsumer> logger) : IConsumer<OrderCancelledIntegrationEvent>
{
    public async Task Consume(ConsumeContext<OrderCancelledIntegrationEvent> context)
    {
        var message = context.Message;

        logger.LogInformation(
            "Processing order cancelled notification for order {OrderId}, customer {CustomerId}",
            message.OrderId, message.CustomerId);

        var notification = NotificationRecord.Create(
            NotificationType.OrderCancelled,
            NotificationChannel.Email,
            recipient: message.CustomerId.ToString(),
            subject: $"Order Cancelled - #{message.OrderId.ToString()[..8]}",
            body: $"Your order #{message.OrderId.ToString()[..8]} has been cancelled. If you did not request this, please contact support.",
            correlationId: message.OrderId);

        try
        {
            await emailSender.SendAsync(
                notification.Recipient,
                notification.Subject,
                notification.Body,
                context.CancellationToken);

            notification.MarkSent();
            metrics.NotificationSent("order_cancelled", "email");

            logger.LogInformation("Order cancelled notification sent for order {OrderId}", message.OrderId);
        }
        catch (Exception ex)
        {
            notification.MarkFailed(ex.Message);

            logger.LogError(ex, "Failed to send order cancelled notification for order {OrderId}", message.OrderId);

            throw;
        }
    }
}
