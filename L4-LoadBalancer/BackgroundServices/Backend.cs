using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace L4_LoadBalancer.BackgroundServices
{
    /// <summary>
    /// Represents a backend server endpoint with associated health state used by the load balancer for routing decisions.
    /// </summary>
    public class Backend
    {
        public EndPoint EndPoint { get; }
        private volatile bool _healthy = true;
        public bool IsHealthy => _healthy;


        private int _activeConnections;
        public int ActiveConnections => Volatile.Read(ref _activeConnections);

        private int _failureCount;

        public Backend(EndPoint ep) => EndPoint = ep;

        public void IncrementConnections() => Interlocked.Increment(ref _activeConnections);
        public void DecrementConnections() => Interlocked.Decrement(ref _activeConnections);

        public void MarkFailure(int threshold)
        {
            if (Interlocked.Increment(ref _failureCount) >= threshold)
                _healthy = false;
        }

        public void MarkSuccess()
        {
            _failureCount = 0;
            _healthy = true;
        }
    }
}
