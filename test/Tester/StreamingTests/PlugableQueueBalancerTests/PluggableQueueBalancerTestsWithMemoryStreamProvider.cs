using Microsoft.Extensions.Configuration;
using Orleans;
using Orleans.Hosting;
using Orleans.Providers;
using Orleans.Runtime.Configuration;
using Orleans.Storage;
using Orleans.TestingHost;
using System.Threading.Tasks;
using TestExtensions;
using Xunit;

namespace Tester.StreamingTests.PlugableQueueBalancerTests
{
    public class PluggableQueueBalancerTestsWithMemoryStreamProvider : PluggableQueueBalancerTestBase, IClassFixture<PluggableQueueBalancerTestsWithMemoryStreamProvider.Fixture>
    {
        private const string StreamProviderName = "MemoryStreamProvider";
        private static readonly int totalQueueCount = 6;
        private static readonly short siloCount = 2;

        private readonly Fixture fixture;

        public class Fixture : BaseTestClusterFixture
        {
            protected override void ConfigureTestCluster(TestClusterBuilder builder)
            {
                builder.Options.InitialSilosCount = siloCount;
                builder.AddSiloBuilderConfigurator<SiloBuilderConfigurator>();
                builder.AddSiloBuilderConfigurator<MySiloBuilderConfigurator>();
                builder.AddClientBuilderConfigurator<MyClientBuilderConfigurator>();
            }

            private class MySiloBuilderConfigurator : ISiloBuilderConfigurator
            {
                public void Configure(ISiloHostBuilder hostBuilder)
                {
                    hostBuilder
                        .AddMemoryStreams<DefaultMemoryMessageBodySerializer>(StreamProviderName, options =>
                        {
                            options.TotalQueueCount = totalQueueCount;
                            options.BalancerType = typeof(LeaseBasedQueueBalancerForTest);
                        })
                        .AddMemoryGrainStorage("PubSubStore");
                }
            }
            
            private class MyClientBuilderConfigurator : IClientBuilderConfigurator
            {
                public void Configure(IConfiguration configuration, IClientBuilder clientBuilder)
                {
                    clientBuilder
                        .AddMemoryStreams<DefaultMemoryMessageBodySerializer>(StreamProviderName, options =>
                        {
                            options.TotalQueueCount = totalQueueCount;
                        });
                }
            }

        }

        public PluggableQueueBalancerTestsWithMemoryStreamProvider(Fixture fixture)
        {
            this.fixture = fixture;
        }

        [Fact, TestCategory("BVT")]
        public Task PluggableQueueBalancerTest_ShouldUseInjectedQueueBalancerAndBalanceCorrectly()
        {
            return base.ShouldUseInjectedQueueBalancerAndBalanceCorrectly(this.fixture, StreamProviderName, siloCount, totalQueueCount);
        }
    }
}
