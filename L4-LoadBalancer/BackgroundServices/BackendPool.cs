using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace L4_LoadBalancer.BackgroundServices
{
    /// <summary>
    /// Maintains a collection of backend servers and provides thread-safe access
    /// to their current health state for use by load-balancing strategies.
    /// </summary>
    public class BackendPool
    {
        private readonly List<Backend> _backends;
        private readonly object _lock = new();

        public BackendPool(List<Backend> backends)
        {
            _backends = backends;
        }

        public IReadOnlyList<Backend> All
        {
            get { lock (_lock) return _backends.ToList(); }
        }

        public IReadOnlyList<Backend> Healthy()
        {
            lock (_lock)
                return _backends.Where(b => b.IsHealthy).ToList();
        }
    }
}
