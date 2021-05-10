using System.Linq;
using System.Net.Mime;
using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;

namespace HealthCheck
{
    public class CustomHealthCheckOptions : HealthCheckOptions
    {
        public CustomHealthCheckOptions(): base()
        {
            var jsonSerializerOptions = new JsonSerializerOptions { WriteIndented = true };
            ResponseWriter = async (httpContext, healthReport) =>
            {
                httpContext.Response.ContentType = MediaTypeNames.Application.Json;
                httpContext.Response.StatusCode = StatusCodes.Status200OK;
                var res = JsonSerializer.Serialize(new
                {
                    checks = healthReport.Entries.Select(e => new
                    {
                        name = e.Key,
                        responseTime = e.Value.Duration.TotalMilliseconds,
                        status = e.Value.Status.ToString(),
                        description = e.Value.Description
                    }),
                    totalStatus = healthReport.Status,
                    totalResponseTime = healthReport.TotalDuration.TotalMilliseconds
                }, jsonSerializerOptions);
                await httpContext.Response.WriteAsync(res);
            };
        }
    }
}