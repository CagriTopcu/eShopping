using Microsoft.Extensions.Diagnostics.HealthChecks;
using Stock.Infrastructure.Persistence;

namespace Stock.Infrastructure.HealthChecks;

internal sealed class PostgresHealthCheck(StockDbContext dbContext) : IHealthCheck
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
