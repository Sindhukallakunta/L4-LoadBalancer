using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace L4_LoadBalancer.Configuration
{
    public class LoadBalancingOptions
    {
        public string Strategy { get; set; } = "RoundRobin";
    }
}
