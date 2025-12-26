using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace L4_LoadBalancer.Configuration
{
    /// <summary>
    /// Configuration model representing a backend server endpoint.
    /// </summary>
    public sealed class BackendConfig
    {
        public string Host { get; set; } = default!;
        public int Port { get; set; }
    }
}
