using System;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
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
}
