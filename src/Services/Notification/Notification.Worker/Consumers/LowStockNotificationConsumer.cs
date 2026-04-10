using MassTransit;
using Microsoft.Extensions.Logging;
using Notification.Worker.Domain;
using Notification.Worker.Services;
using Shared.BuildingBlocks.Observability;
using Shared.Contracts.Events.Stock;

namespace Notification.Worker.Consumers;

public sealed class LowStockNotificationConsumer(
    IEmailSender emailSender,
    BusinessMetrics metrics,
    ILogger<LowStockNotificationConsumer> logger) : IConsumer<LowStockWarningEvent>
{
    public async Task Consume(ConsumeContext<LowStockWarningEvent> context)
    {
        var message = context.Message;

        logger.LogInformation(
            "Processing low stock notification for product {ProductId}, remaining: {Remaining}, threshold: {Threshold}",
            message.ProductId, message.RemainingQuantity, message.Threshold);

        var notification = NotificationRecord.Create(
            NotificationType.LowStock,
            NotificationChannel.Email,
            recipient: "admin@eshopping.local",
            subject: $"Low Stock Alert - Product {message.ProductId.ToString()[..8]}",
            body: $"Product {message.ProductId} has only {message.RemainingQuantity} units remaining (threshold: {message.Threshold}).",
            correlationId: message.ProductId);

        try
        {
            await emailSender.SendAsync(
                notification.Recipient,
                notification.Subject,
                notification.Body,
                context.CancellationToken);

            notification.MarkSent();
            metrics.NotificationSent("low_stock", "email");

            logger.LogInformation("Low stock notification sent for product {ProductId}", message.ProductId);
        }
        catch (Exception ex)
        {
            notification.MarkFailed(ex.Message);

            logger.LogError(ex, "Failed to send low stock notification for product {ProductId}", message.ProductId);

            throw;
        }
    }
}
