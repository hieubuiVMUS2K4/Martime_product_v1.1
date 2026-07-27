using Microsoft.Extensions.Diagnostics.HealthChecks;
using MaritimeEdge.Data;

namespace MaritimeEdge.Security
{
    public class EdgeDatabaseHealthCheck : IHealthCheck
    {
        private readonly EdgeDbContext _dbContext;

        public EdgeDatabaseHealthCheck(EdgeDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var canConnect = await _dbContext.Database.CanConnectAsync(cancellationToken);
                return canConnect
                    ? HealthCheckResult.Healthy("Edge Database connection is healthy.")
                    : HealthCheckResult.Unhealthy("Cannot connect to Edge PostgreSQL database.");
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy("Edge database health check failed.", ex);
            }
        }
    }
}
