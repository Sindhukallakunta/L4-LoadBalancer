using L4_LoadBalancer.BackgroundServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace L4_LoadBalancer.Core
{
    /// <summary>
    /// Contract for selecting a backend server from a collection of healthy backends
    /// </summary>
    public interface ILoadBalancingStrategy
    {
        Backend Pick(IReadOnlyList<Backend> backends);
    }
}
