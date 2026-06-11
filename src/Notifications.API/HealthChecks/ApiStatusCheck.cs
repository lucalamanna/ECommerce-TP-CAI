using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Notifications.API.HealthChecks;

public class ApiStatusCheck : IHealthCheck
{
    private static readonly DateTime StartTime = DateTime.UtcNow;

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var uptime = DateTime.UtcNow - StartTime;
        var data = new Dictionary<string, object>
        {
            ["uptime"] = uptime.ToString(@"hh\:mm\:ss"),
            ["dotnetVersion"] = Environment.Version.ToString(),
            ["startTime"] = StartTime.ToString("o")
        };
        return Task.FromResult(HealthCheckResult.Healthy("API operativa", data));
    }
}