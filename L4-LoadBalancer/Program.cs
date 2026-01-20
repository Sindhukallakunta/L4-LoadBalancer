
using L4_LoadBalancer.BackgroundServices;
using L4_LoadBalancer.Configuration;
using L4_LoadBalancer.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Serilog;


var configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false)
    .Build();

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configuration)
    .CreateLogger();
var services = new ServiceCollection();

services.AddLogging(builder =>
{
    builder.ClearProviders();
    builder.AddSerilog();
});


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
services.AddSingleton<TcpLoadBalancer>();
services.AddSingleton<IBackendHealthProbe, TcpBackendHealthProbe>();

services.AddSingleton(provider =>
    new HealthCheckService(
        provider.GetRequiredService<BackendPool>(),
        TimeSpan.FromSeconds(healthOptions.IntervalSeconds),
        provider.GetRequiredService<ILogger<HealthCheckService>>(),
        provider.GetRequiredService<IBackendHealthProbe>()
    )
);


using var provider = services.BuildServiceProvider();

var lb = provider.GetRequiredService<TcpLoadBalancer>();
var healthChecker = provider.GetRequiredService<HealthCheckService>();

 var cts = new CancellationTokenSource();

Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    cts.Cancel();
};


AppDomain.CurrentDomain.ProcessExit += (_, _) =>
{
    cts.Cancel();
};

var healthTask = Task.Run(() => healthChecker.RunAsync(cts.Token));

var lbTask= lb.RunAsync(cts.Token);

await lbTask;

await healthTask;