using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Stock.Infrastructure.Persistence;

/// <summary>
/// Ensures the Stock database schema exists on startup.
/// Runs as a background service so the application starts and passes health checks
/// even if the database isn't immediately available. Retries with backoff.
/// </summary>
internal sealed class StockDbInitializer(
    IServiceProvider serviceProvider,
    ILogger<StockDbInitializer> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<StockDbContext>();

        for (var attempt = 1; attempt <= 5; attempt++)
        {
            try
            {
                await db.Database.EnsureCreatedAsync(cancellationToken);
                logger.LogInformation("Stock database schema ready.");
                return;
            }
            catch (Exception ex) when (attempt < 5)
            {
                var delay = TimeSpan.FromSeconds(attempt * 2);
                logger.LogWarning(ex,
                    "Stock database initialization attempt {Attempt}/5 failed. Retrying in {Delay}s.",
                    attempt, delay.TotalSeconds);
                await Task.Delay(delay, cancellationToken);
            }
        }

        logger.LogError("Stock database initialization failed after 5 attempts. Requests requiring database access will fail until the database is available.");
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
