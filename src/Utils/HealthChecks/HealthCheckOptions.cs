using System.Linq;
using System.Net.Mime;
using System.Reflection;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Newtonsoft.Json;

namespace form_builder.Utils.HealthChecks
{
    public static class HealthCheckConfig
    {
        private static readonly AssemblyName _assembly = Assembly.GetEntryAssembly().GetName();

        public static HealthCheckOptions Options => new HealthCheckOptions
        {
            ResponseWriter = async (c, r) =>
            {
                c.Response.ContentType = MediaTypeNames.Application.Json;

                switch (r.Status)
                {
                    case HealthStatus.Healthy:
                        await c.Response.WriteAsync(ProcessHealthy(r));
                        break;
                    case HealthStatus.Unhealthy:
                        await c.Response.WriteAsync(ProcessUnhealthy(r));
                        break;
                    default:
                        break;
                }
            }
        };

        private static string ProcessUnhealthy(HealthReport report)
        {
            return JsonConvert.SerializeObject(new
                {
                    application = new {
                        name = _assembly.Name,
                        version = _assembly.Version.ToString(),
                        status = report.Status.ToString(),
                    },
                    checks = report.Entries.Select(e =>
                        new {
                            description = e.Key,
                            status = e.Value.Status.ToString(),
                            exception = e.Value.Exception?.Message,
                            data = e.Value.Data.Select(_ => $"{_.Key}: {_.Value}"),
                            responseTime = e.Value.Duration.TotalMilliseconds
                        }),
                    totalResponseTime = report.TotalDuration.TotalMilliseconds
                });
        }

        private static string ProcessHealthy(HealthReport report)
        {
            return JsonConvert.SerializeObject(new
                {
                    application = new {
                        name = _assembly.Name,
                        version = _assembly.Version.ToString(),
                        status = report.Status.ToString(),
                    },
                    checks = report.Entries.Select(e =>
                        new {
                            description = e.Key,
                            status = e.Value.Status.ToString(),
                            responseTime = e.Value.Duration.TotalMilliseconds
                        }),
                    totalResponseTime = report.TotalDuration.TotalMilliseconds
                });
        }
    }
}