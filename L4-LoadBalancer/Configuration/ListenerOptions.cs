using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace L4_LoadBalancer.Configuration
{
    public class ListenerOptions
    {
        public string Address { get; set; } = "0.0.0.0";
        public int Port { get; set; }
    }
}
