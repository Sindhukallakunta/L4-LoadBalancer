using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace L4_LoadBalancer.BackgroundServices
{
    /// <summary>
    /// Performs TCP-based connectivity checks against backend endpoints to determine basic availability
    /// </summary>
    public class TcpBackendHealthProbe:IBackendHealthProbe
    {
        public async Task<bool> ProbeAsync(Backend backend, CancellationToken ct)
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
