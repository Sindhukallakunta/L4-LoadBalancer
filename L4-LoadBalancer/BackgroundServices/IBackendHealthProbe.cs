using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace L4_LoadBalancer.BackgroundServices
{
    /// <summary>
    /// Contract for performing health checks against backend servers.
    /// </summary>
    public interface IBackendHealthProbe
    {
        Task<bool> ProbeAsync(Backend backend, CancellationToken ct);
    }
}
