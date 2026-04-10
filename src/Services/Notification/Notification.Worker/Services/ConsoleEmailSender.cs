using Microsoft.Extensions.Logging;

namespace Notification.Worker.Services;

public sealed class ConsoleEmailSender(ILogger<ConsoleEmailSender> logger) : IEmailSender
{
    public Task SendAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "[EMAIL] To: {Recipient} | Subject: {Subject} | Body: {Body}",
            to, subject, body);

        return Task.CompletedTask;
    }
}
