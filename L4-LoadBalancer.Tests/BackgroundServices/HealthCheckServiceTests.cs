using FluentAssertions;
using L4_LoadBalancer.BackgroundServices;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace L4_LoadBalancer.Tests.BackgroundServices
{
    public class HealthCheckServiceTests
    {
        private BackendPool _pool;
        private List<Backend> _backends;
        private Mock<IBackendHealthProbe> _probe;
        private HealthCheckService _service;

        [SetUp]
        public void SetUp()
        {
            _backends = new List<Backend>
        {
            new Backend(new DnsEndPoint("a", 1)),
            new Backend(new DnsEndPoint("b", 2))
        };

            _pool = new BackendPool(_backends);
            _probe = new Mock<IBackendHealthProbe>();

            var logger = Mock.Of<ILogger<HealthCheckService>>();

            _service = new HealthCheckService(
                _pool,
                TimeSpan.FromMilliseconds(10),
                logger,
                _probe.Object,
                failureThreshold: 1);
        }

        [Test]
        public async Task RunAsync_Should_Mark_Backend_Unhealthy_On_Failure()
        {
            _probe
                .Setup(p => p.ProbeAsync(It.IsAny<Backend>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            using var cts = new CancellationTokenSource();
            cts.CancelAfter(50);

            await _service.RunAsync(cts.Token);

            _backends.All(b => b.IsHealthy).Should().BeFalse();
        }


        [Test]
        public async Task RunAsync_Should_Mark_Backend_Healthy_On_Success()
        {
            _probe
                .Setup(p => p.ProbeAsync(It.IsAny<Backend>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            using var cts = new CancellationTokenSource();
            cts.CancelAfter(50);

            await _service.RunAsync(cts.Token);

            _backends.All(b => b.IsHealthy).Should().BeTrue();
        }

        [Test]
        public async Task RunAsync_Should_Stop_Gracefully_On_Cancellation()
        {
            _probe
                .Setup(p => p.ProbeAsync(It.IsAny<Backend>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            using var cts = new CancellationTokenSource();
            cts.Cancel();

            await _service.RunAsync(cts.Token);

            // No exception = pass
        }

    }
}
