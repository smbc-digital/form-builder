using System.Net.Mime;
using System.Reflection;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Newtonsoft.Json;

namespace form_builder.Utils.HealthChecks
{
    public static class HealthCheckConfig
    {
        private static readonly AssemblyName _assembly = Assembly.GetEntryAssembly().GetName();

        public static HealthCheckOptions Options => new HealthCheckOptions
        {
            ResponseWriter = async (context, report) =>
            {
                context.Response.ContentType = MediaTypeNames.Application.Json;

                switch (report.Status)
                {
                    case HealthStatus.Healthy:
                        await context.Response.WriteAsync(ProcessHealthy(report));
                        break;

                    case HealthStatus.Unhealthy:
                        await context.Response.WriteAsync(ProcessUnhealthy(report));
                        break;

                    default:
                        break;
                }
            }
        };

        private static string ProcessUnhealthy(HealthReport report) =>
            JsonConvert.SerializeObject(new
            {
                application = new
                {
                    name = _assembly.Name,
                    version = _assembly.Version.ToString(),
                    status = report.Status.ToString(),
                },
                checks = report.Entries.Select(e =>
                    new
                    {
                        description = e.Key,
                        status = e.Value.Status.ToString(),
                        exception = e.Value.Exception?.Message,
                        data = e.Value.Data.Select(_ => $"{_.Key}: {_.Value}"),
                        responseTime = e.Value.Duration.TotalMilliseconds
                    }),
                totalResponseTime = report.TotalDuration.TotalMilliseconds
            });

        private static string ProcessHealthy(HealthReport report) =>
            JsonConvert.SerializeObject(new
            {
                application = new
                {
                    name = _assembly.Name,
                    version = _assembly.Version.ToString(),
                    status = report.Status.ToString(),
                },
                checks = report.Entries.Select(e =>
                    new
                    {
                        description = e.Key,
                        status = e.Value.Status.ToString(),
                        responseTime = e.Value.Duration.TotalMilliseconds
                    }),
                totalResponseTime = report.TotalDuration.TotalMilliseconds
            });
    }
}