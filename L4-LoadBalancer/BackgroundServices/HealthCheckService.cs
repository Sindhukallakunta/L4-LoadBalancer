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
        private readonly IBackendHealthProbe _probe;
        public HealthCheckService(BackendPool pool,TimeSpan interval, ILogger<HealthCheckService> logger,IBackendHealthProbe probe,int failureThreshold = 3)
        {
            _pool = pool;
            _interval = interval;
            _logger = logger;
            _probe = probe;
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
                        bool ok = await _probe.ProbeAsync(backend, ct);

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
        
    }
}
