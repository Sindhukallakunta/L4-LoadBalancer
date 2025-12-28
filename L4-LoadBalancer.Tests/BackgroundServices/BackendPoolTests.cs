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
    public class BackendPoolTests
    {
        private List<Backend> _backends;
        private BackendPool _pool;
        [SetUp]
        public void Setup()
        {
            _backends = new List<Backend> { new Backend(new DnsEndPoint("localhost", 9001)),
                                            new Backend(new DnsEndPoint("localhost", 9002)), 
                                            new Backend(new DnsEndPoint("localhost", 9003))};
            _pool=new BackendPool(_backends);

        }

        [Test]
        public void All_Should_Return_All_Backends()
        {
            _pool.All.Should().HaveCount(3);
        }

        [Test]
        public void Healthy_Should_Return_All_Backends_When_All_Are_Healthy()
        {
            _pool.Healthy().Should().HaveCount(3);
        }

        [Test]
        public void Healthy_Should_Exclude_Unhealthy_Backends()
        {
            _backends.First().MarkFailure(threshold: 1);

            _pool.Healthy().Should().HaveCount(2);
        }

    }
}
