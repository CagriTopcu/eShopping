using Microsoft.Extensions.Diagnostics.HealthChecks;
using Payment.Infrastructure.Persistence;

namespace Payment.Infrastructure.HealthChecks;

internal sealed class PostgresHealthCheck(PaymentDbContext dbContext) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await dbContext.Database.CanConnectAsync(cancellationToken);
            return HealthCheckResult.Healthy("PostgreSQL is reachable.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("PostgreSQL connection failed.", ex);
        }
    }
}
