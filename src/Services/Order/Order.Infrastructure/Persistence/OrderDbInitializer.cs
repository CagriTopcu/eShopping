using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Order.Infrastructure.Persistence;

internal sealed class OrderDbInitializer(
    IServiceProvider serviceProvider,
    ILogger<OrderDbInitializer> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<OrderDbContext>();

        for (var attempt = 1; attempt <= 5; attempt++)
        {
            try
            {
                await db.Database.EnsureCreatedAsync(cancellationToken);
                logger.LogInformation("Order database schema ready.");
                return;
            }
            catch (Exception ex) when (attempt < 5)
            {
                var delay = TimeSpan.FromSeconds(attempt * 2);
                logger.LogWarning(ex,
                    "Order database initialization attempt {Attempt}/5 failed. Retrying in {Delay}s.",
                    attempt, delay.TotalSeconds);
                await Task.Delay(delay, cancellationToken);
            }
        }

        logger.LogError("Order database initialization failed after 5 attempts. Requests requiring database access will fail until the database is available.");
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
