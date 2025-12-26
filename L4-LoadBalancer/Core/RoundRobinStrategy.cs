using L4_LoadBalancer.BackgroundServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace L4_LoadBalancer.Core
{

    /// <summary>
    /// Load-balancing strategy that selects backends in a round-robin manner,
    /// ensuring sequential and thread-safe distribution across available backends.
    /// </summary>
    public class RoundRobinStrategy : ILoadBalancingStrategy
    {
        private int _idx = -1;
        public Backend Pick(IReadOnlyList<Backend> backends)
        {
            if (backends.Count == 0) throw new InvalidOperationException("No healthy backends");
            var i = Interlocked.Increment(ref _idx);
            return backends[i % backends.Count];
        }
    }
}
