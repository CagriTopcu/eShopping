namespace Notification.Worker.Domain;

public enum NotificationType
{
    OrderConfirmed,
    OrderCancelled,
    PaymentFailed,
    LowStock
}
