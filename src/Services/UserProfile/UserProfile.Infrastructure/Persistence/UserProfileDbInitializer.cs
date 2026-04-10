using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace UserProfile.Infrastructure.Persistence;

internal sealed class UserProfileDbInitializer(
    IServiceProvider serviceProvider,
    ILogger<UserProfileDbInitializer> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<UserProfileDbContext>();

        for (var attempt = 1; attempt <= 5; attempt++)
        {
            try
            {
                await db.Database.EnsureCreatedAsync(cancellationToken);
                logger.LogInformation("UserProfile database schema ready.");
                return;
            }
            catch (Exception ex) when (attempt < 5)
            {
                var delay = TimeSpan.FromSeconds(attempt * 2);
                logger.LogWarning(ex,
                    "UserProfile database initialization attempt {Attempt}/5 failed. Retrying in {Delay}s.",
                    attempt, delay.TotalSeconds);
                await Task.Delay(delay, cancellationToken);
            }
        }

        logger.LogError("UserProfile database initialization failed after 5 attempts.");
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
