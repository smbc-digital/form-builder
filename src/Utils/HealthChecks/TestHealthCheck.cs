using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace form_builder.Utils.HealthChecks
{
    public class TestHealthCheck : IHealthCheck
    {
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default) =>
            await Task.FromResult(HealthCheckResult.Healthy(null, new Dictionary<string, dynamic> { { "Result", "All working!" } }));
    }
}