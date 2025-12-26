using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace L4_LoadBalancer.BackgroundServices
{
    /// <summary>
    /// Background service that performs periodic TCP health checks on backend servers
    /// and updates their health status based on configurable failure thresholds.
    /// </summary>
    public class HealthCheckService
    {
        private readonly BackendPool _pool;
        private readonly TimeSpan _interval;
        private readonly int _failureThreshold;
        private readonly ILogger<HealthCheckService> _logger;
        public HealthCheckService(BackendPool pool,TimeSpan interval, ILogger<HealthCheckService> logger,int failureThreshold = 3)
        {
            _pool = pool;
            _interval = interval;
            _logger = logger;
            _failureThreshold = failureThreshold;
        }

        public async Task RunAsync(CancellationToken ct)
        {
           // _logger.LogInformation("HealthChecker started");
            try
            {
                while (!ct.IsCancellationRequested)
                {
                    foreach (var backend in _pool.All)
                    {
                        bool ok = await TcpProbeAsync(backend, ct);

                        if (ok)
                            backend.MarkSuccess();
                        else
                            backend.MarkFailure(_failureThreshold);
                    }

                    await Task.Delay(_interval, ct);
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("HealthChecker stopped gracefully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "HealthChecker crashed unexpectedly");
            }
        }

        private static async Task<bool> TcpProbeAsync(Backend backend, CancellationToken ct)
        {
            try
            {
                if (backend.EndPoint is not DnsEndPoint dns)
                    return false;

                using var client = new TcpClient();
                using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                timeoutCts.CancelAfter(500);

                await client.ConnectAsync(dns.Host, dns.Port, timeoutCts.Token);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
