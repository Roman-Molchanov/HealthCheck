using System;
using System.Linq;
using System.Net.Mime;
using System.Net.NetworkInformation;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthCheck
{
    public class ICMPHealthCheck: IHealthCheck
    {
        private readonly string host;
        private readonly int healthyRoundtripTime;

        public ICMPHealthCheck(string host, int healthyRoundtripTime)
        {
            this.host = host;
            this.healthyRoundtripTime = healthyRoundtripTime;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                using var ping = new Ping();
                var reply = await ping.SendPingAsync(host);
                var description = $"ICMP to {host} took {reply.RoundtripTime} ms."; 
                return reply.Status switch
                {
                    IPStatus.Success => reply.RoundtripTime > healthyRoundtripTime
                        ? HealthCheckResult.Degraded(description)
                        : HealthCheckResult.Healthy(description),
                    _ => HealthCheckResult.Unhealthy($"ICMP to {host} failed: {reply.Status}"),
                };
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy($"ICMP to {host} failed: {ex.Message}", ex);
            }
        }
    }

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
