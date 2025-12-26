using L4_LoadBalancer.BackgroundServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace L4_LoadBalancer.Configuration
{
    /// <summary>
    /// Loads backend server configuration from a JSON configuration file
    /// and constructs backend definitions for use by the load balancer.
    /// </summary>
    public class BackendConfigLoader
    {
        public static List<Backend> LoadBackendsFromJson(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException($"Backend config not found: {path}");

            var json = File.ReadAllText(path);

            var configs = JsonSerializer.Deserialize<List<BackendConfig>>(json)
                          ?? throw new InvalidOperationException("Invalid backend config");

            return configs.Select(c =>
                new Backend(new DnsEndPoint(c.Host, c.Port))
            ).ToList();
        }
    }
}
