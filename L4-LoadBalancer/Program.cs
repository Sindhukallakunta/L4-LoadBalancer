
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
using Microsoft.Extensions.Options;


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

services.AddOptions<ListenerOptions>()
    .Bind(configuration.GetSection("Listener"))
    .Validate(o => o.Port is > 0 and < 65536, "Listener port must be valid")
    .Validate(o => IPAddress.TryParse(o.Address, out _),
              "Listener address must be a valid IP address")
    .ValidateOnStart();

services.AddOptions<LoadBalancingOptions>()
    .Bind(configuration.GetSection("LoadBalancing"))
    .Validate(o => !string.IsNullOrWhiteSpace(o.Strategy),
              "LoadBalancing:Strategy must be configured")
    .ValidateOnStart();

services.AddSingleton<IPEndPoint>(sp =>
{
    var options = sp.GetRequiredService<IOptions<ListenerOptions>>().Value;
    return new IPEndPoint(IPAddress.Parse(options.Address), options.Port);
});

services.AddSingleton(new BackendPool(backends));
services.AddSingleton<RoundRobinStrategy>();

services.AddSingleton<ILoadBalancingStrategy>(sp =>
{
    var options = sp.GetRequiredService<IOptions<LoadBalancingOptions>>().Value;

    return options.Strategy.ToLower() switch
    {
        "roundrobin" =>
            sp.GetRequiredService<RoundRobinStrategy>(),        

        _ => throw new InvalidOperationException(
            $"Unknown load balancing strategy '{options.Strategy}'")
    };
});
services.AddSingleton<TcpLoadBalancer>();
services.AddSingleton<IBackendHealthProbe, TcpBackendHealthProbe>();

services.AddSingleton(provider =>
    new HealthCheckService(
        provider.GetRequiredService<BackendPool>(),
        TimeSpan.FromSeconds(healthOptions==null?0:healthOptions.IntervalSeconds),
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