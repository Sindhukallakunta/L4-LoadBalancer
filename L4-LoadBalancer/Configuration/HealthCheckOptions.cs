using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace L4_LoadBalancer.Configuration
{
    /// <summary>
    /// Configuration options that control health check behaviour
    /// </summary>
    public class HealthCheckOptions
    {
        public int IntervalSeconds { get; set; } = 0;
        public int FailureThreshold { get; set; }
    }
}
