using FluentAssertions;
using L4_LoadBalancer.BackgroundServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace L4_LoadBalancer.Tests.BackgroundServices
{
    [TestFixture]
    public class BackendTests
    {
        private Backend _backend;

        [SetUp]
        public void SetUp()
        {
            _backend = new Backend(new DnsEndPoint("localhost", 9001));
        }

        [Test]
        public void Backend_Should_Be_Healthy_By_Default()
        {
            _backend.IsHealthy.Should().BeTrue();
        }

        [Test]
        public void Backend_Should_Remain_Healthy_Before_Failure_Threshold()
        {
            _backend.MarkFailure(threshold: 3);
            _backend.MarkFailure(threshold: 3);

            _backend.IsHealthy.Should().BeTrue();
        }

        [Test]
        public void Backend_Should_Become_Unhealthy_After_Reaching_Failure_Threshold()
        {
            _backend.MarkFailure(threshold: 3);
            _backend.MarkFailure(threshold: 3);
            _backend.MarkFailure(threshold: 3);

            _backend.IsHealthy.Should().BeFalse();
        }

        [Test]
        public void Backend_Should_Recover_When_Marked_Successful()
        {
            _backend.MarkFailure(threshold: 2);
            _backend.MarkFailure(threshold: 2);

            _backend.IsHealthy.Should().BeFalse();

            _backend.MarkSuccess();

            _backend.IsHealthy.Should().BeTrue();
        }

        [Test]
        public void Backend_Should_Reset_Failure_Count_On_Success()
        {
            _backend.MarkFailure(threshold: 3);
            _backend.MarkFailure(threshold: 3);

            _backend.MarkSuccess();

            _backend.MarkFailure(threshold: 3);
            _backend.MarkFailure(threshold: 3);

            _backend.IsHealthy.Should().BeTrue();
        }
    }
}



