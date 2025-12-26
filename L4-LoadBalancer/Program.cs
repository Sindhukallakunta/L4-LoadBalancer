
using L4_LoadBalancer.BackgroundServices;
using L4_LoadBalancer.Configuration;
using L4_LoadBalancer.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;

var services = new ServiceCollection();

services.AddLogging(builder =>
{
    builder.AddConsole().SetMinimumLevel(LogLevel.Information);
});

var configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false)
    .Build();

var healthOptions = configuration
    .GetSection("HealthCheck")
    .Get<HealthCheckOptions>();

var backends = BackendConfigLoader.LoadBackendsFromJson(Path.Combine(
    AppContext.BaseDirectory,
    "Configuration",
    "backends.json"));

services.AddSingleton(
    new IPEndPoint(IPAddress.Any, 9000)
);
services.AddSingleton(new BackendPool(backends));
services.AddSingleton<ILoadBalancingStrategy, RoundRobinStrategy>();
services.AddSingleton<TcpConnectionListener>();

services.AddSingleton(provider =>
    new HealthCheckService(
        provider.GetRequiredService<BackendPool>(),
        TimeSpan.FromSeconds(healthOptions.IntervalSeconds),
        provider.GetRequiredService<ILogger<HealthCheckService>>()
    )
);


using var provider = services.BuildServiceProvider();

var lb = provider.GetRequiredService<TcpConnectionListener>();
var healthChecker = provider.GetRequiredService<HealthCheckService>();

var cts = new CancellationTokenSource();

_ = Task.Run(() => healthChecker.RunAsync(cts.Token));

var ctsLB = new CancellationTokenSource();
await lb.RunAsync(ctsLB.Token);