using FluentAssertions;
using L4_LoadBalancer.BackgroundServices;
using L4_LoadBalancer.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace L4_LoadBalancer.Tests.Core
{
    public class RoundRobinStrategyTests
    {
        private RoundRobinStrategy _strategy;
        private List<Backend> _backends;

        [SetUp]
        public void SetUp()
        {
            _strategy = new RoundRobinStrategy();
            _backends = new List<Backend>
            {
                new Backend(new DnsEndPoint("a", 1)),
                new Backend(new DnsEndPoint("b", 2)),
                new Backend(new DnsEndPoint("c", 3))
            };
        }

        [Test]
        public void Pick_Should_Rotate_Backends_In_Order()
        {
            var first = _strategy.Pick(_backends);
            var second = _strategy.Pick(_backends);
            var third = _strategy.Pick(_backends);

            first.Should().Be(_backends[0]);
            second.Should().Be(_backends[1]);
            third.Should().Be(_backends[2]);
        }

        [Test]
        public async Task Pick_Should_Distribute_Backends_Under_Concurrent_Access()
        {           

            var results = new ConcurrentBag<Backend>();
            var tasks = new List<Task>();

            // Act: simulate concurrent requests
            for (int i = 0; i < 100; i++)
            {
                tasks.Add(Task.Run(() =>
                {
                    var backend = _strategy.Pick(_backends);
                    results.Add(backend);
                }));
            }

            await Task.WhenAll(tasks);

            // Assert
            results.Should().NotBeEmpty();
            results.Should().OnlyContain(b => _backends.Contains(b));
            results.Count.Should().Be(100);
            results.GroupBy(b => b)
                   .Select(g => g.Count())
                   .Should()
                   .OnlyContain(count => count > 0);
        }

        [Test]
        public void Pick_Should_Throw_When_No_Backends_Available()
        {
            var strategy = new RoundRobinStrategy();
            var backends = new List<Backend>();

            Action act = () => strategy.Pick(backends);

            act.Should().Throw<InvalidOperationException>()
               .WithMessage("No healthy backends");
        }


    }
}
