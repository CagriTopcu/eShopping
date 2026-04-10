namespace Notification.Worker.Domain;

public sealed class NotificationRecord
{
    public Guid Id { get; private set; }
    public NotificationType Type { get; private set; }
    public NotificationChannel Channel { get; private set; }
    public NotificationStatus Status { get; private set; }
    public string Recipient { get; private set; } = null!;
    public string Subject { get; private set; } = null!;
    public string Body { get; private set; } = null!;
    public Guid? CorrelationId { get; private set; }
    public int RetryCount { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? SentAt { get; private set; }
    public string? LastError { get; private set; }

    private NotificationRecord() { }

    public static NotificationRecord Create(
        NotificationType type,
        NotificationChannel channel,
        string recipient,
        string subject,
        string body,
        Guid? correlationId = null) => new()
    {
        Id = Guid.NewGuid(),
        Type = type,
        Channel = channel,
        Status = NotificationStatus.Pending,
        Recipient = recipient,
        Subject = subject,
        Body = body,
        CorrelationId = correlationId,
        RetryCount = 0,
        CreatedAt = DateTimeOffset.UtcNow
    };

    public void MarkSent()
    {
        Status = NotificationStatus.Sent;
        SentAt = DateTimeOffset.UtcNow;
    }

    public void MarkFailed(string error)
    {
        Status = NotificationStatus.Failed;
        LastError = error;
        RetryCount++;
    }
}
