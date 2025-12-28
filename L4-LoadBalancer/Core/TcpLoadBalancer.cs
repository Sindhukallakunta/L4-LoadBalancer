using System.Net;
using System.Net.Sockets;
using L4_LoadBalancer.BackgroundServices;
using Microsoft.Extensions.Logging;

namespace L4_LoadBalancer.Core
{
    /// <summary>
    /// Listens for incoming TCP client connections and proxies traffic to backend servers using a load-balancing strategy
    /// </summary>
    public class TcpLoadBalancer
    {
        private readonly IPEndPoint _listen;
        private readonly BackendPool _pool;
        private readonly ILoadBalancingStrategy _strategy;
        private readonly ILogger<TcpLoadBalancer> _logger;

        private TcpListener? _listener;
        private int _activeConnections;

        public TcpLoadBalancer(IPEndPoint listen,BackendPool pool,ILoadBalancingStrategy strategy,ILogger<TcpLoadBalancer> logger)
        {
            _listen = listen;
            _pool = pool;
            _strategy = strategy;
            _logger = logger;
        }

        /// <summary>
        /// This method will listen and fetch requests and passes them 
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task RunAsync(CancellationToken ct)
        {
            _listener = new TcpListener(_listen);
            _listener.Start();
            _logger.LogInformation("Load balancer listening on {Address}",_listen);

            try
            {
                while (!ct.IsCancellationRequested)
                {
                    var client = await _listener.AcceptTcpClientAsync(ct);
                    Interlocked.Increment(ref _activeConnections);

                    _ = Task.Run(() => HandleClientAsync(client, ct));
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Shutdown signal received");
            }
            finally
            {
                _listener.Stop();
                _logger.LogInformation("Stopped accepting new connections. Waiting for {Count} active connections to finish",_activeConnections);

                while (Volatile.Read(ref _activeConnections) > 0)
                {
                    await Task.Delay(100);
                }

                _logger.LogInformation("Load balancer shutdown complete");
            }
        }

        /// <summary>
        /// This method receives the request and using strategy it will pick a backend server amoung pool which has healthy server list and transports it to the picked server
        /// </summary>
        /// <param name="client"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        private async Task HandleClientAsync(TcpClient client, CancellationToken ct)
        {
            try
            {
                var backend = _strategy.Pick(_pool.Healthy());

                _logger.LogInformation("Routing connection to backend {Backend}",backend.EndPoint);

                using var upstream = new TcpClient();

                if (backend.EndPoint is DnsEndPoint dns)
                    await upstream.ConnectAsync(dns.Host, dns.Port, ct);

                using var clientStream = client.GetStream();
                using var backendStream = upstream.GetStream();

                var t1 = clientStream.CopyToAsync(backendStream, ct);
                var t2 = backendStream.CopyToAsync(clientStream, ct);

                await Task.WhenAny(t1, t2);
            }
            catch (OperationCanceledException)
            {
                _logger.LogDebug("Connection cancelled");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Connection handling failed");
            }
            finally
            {
                client.Close();
                Interlocked.Decrement(ref _activeConnections);
            }
        }
    }
}
