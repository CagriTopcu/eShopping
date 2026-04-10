using MassTransit;
using Microsoft.Extensions.Logging;
using Notification.Worker.Domain;
using Notification.Worker.Services;
using Shared.BuildingBlocks.Observability;
using Shared.Contracts.Events.Orders;

namespace Notification.Worker.Consumers;

public sealed class OrderConfirmedNotificationConsumer(
    IEmailSender emailSender,
    BusinessMetrics metrics,
    ILogger<OrderConfirmedNotificationConsumer> logger) : IConsumer<OrderConfirmedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<OrderConfirmedIntegrationEvent> context)
    {
        var message = context.Message;

        logger.LogInformation(
            "Processing order confirmed notification for order {OrderId}, customer {CustomerId}",
            message.OrderId, message.CustomerId);

        var notification = NotificationRecord.Create(
            NotificationType.OrderConfirmed,
            NotificationChannel.Email,
            recipient: message.CustomerId.ToString(),
            subject: $"Order Confirmed - #{message.OrderId.ToString()[..8]}",
            body: $"Your order has been confirmed. Total: {message.TotalAmount:C}. Thank you for shopping with us!",
            correlationId: message.OrderId);

        try
        {
            await emailSender.SendAsync(
                notification.Recipient,
                notification.Subject,
                notification.Body,
                context.CancellationToken);

            notification.MarkSent();
            metrics.NotificationSent("order_confirmed", "email");

            logger.LogInformation("Order confirmed notification sent for order {OrderId}", message.OrderId);
        }
        catch (Exception ex)
        {
            notification.MarkFailed(ex.Message);

            logger.LogError(ex, "Failed to send order confirmed notification for order {OrderId}", message.OrderId);

            throw; // Let MassTransit retry handle it
        }
    }
}
