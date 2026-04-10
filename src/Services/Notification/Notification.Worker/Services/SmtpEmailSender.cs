using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Notification.Worker.Services;

public sealed class SmtpEmailSender(
    IConfiguration configuration,
    ILogger<SmtpEmailSender> logger) : IEmailSender
{
    public async Task SendAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
    {
        var smtpHost = configuration["Smtp:Host"] ?? throw new InvalidOperationException("Smtp:Host is not configured.");
        var smtpPort = int.Parse(configuration["Smtp:Port"] ?? "587");
        var smtpUser = configuration["Smtp:Username"] ?? throw new InvalidOperationException("Smtp:Username is not configured.");
        var smtpPass = configuration["Smtp:Password"] ?? throw new InvalidOperationException("Smtp:Password is not configured.");
        var fromAddress = configuration["Smtp:FromAddress"] ?? smtpUser;

        using var client = new SmtpClient(smtpHost, smtpPort)
        {
            Credentials = new NetworkCredential(smtpUser, smtpPass),
            EnableSsl = true
        };

        var message = new MailMessage(fromAddress, to, subject, body) { IsBodyHtml = true };

        await client.SendMailAsync(message, cancellationToken);

        logger.LogInformation("Email sent to {Recipient} with subject {Subject}", to, subject);
    }
}
