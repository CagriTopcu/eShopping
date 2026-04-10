using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Shipping.Infrastructure.Persistence;

internal sealed class ShippingDbInitializer(
    IServiceProvider serviceProvider,
    ILogger<ShippingDbInitializer> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ShippingDbContext>();

        for (var attempt = 1; attempt <= 5; attempt++)
        {
            try
            {
                await db.Database.EnsureCreatedAsync(cancellationToken);
                logger.LogInformation("Shipping database schema ready.");
                return;
            }
            catch (Exception ex) when (attempt < 5)
            {
                var delay = TimeSpan.FromSeconds(attempt * 2);
                logger.LogWarning(ex,
                    "Shipping database initialization attempt {Attempt}/5 failed. Retrying in {Delay}s.",
                    attempt, delay.TotalSeconds);
                await Task.Delay(delay, cancellationToken);
            }
        }

        logger.LogError("Shipping database initialization failed after 5 attempts.");
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
