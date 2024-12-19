//
// Copyright The Microcks Authors.
//
// Licensed under the Apache License, Version 2.0 (the "License")
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//  http://www.apache.org/licenses/LICENSE-2.0 
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
//


using DotNet.Testcontainers.Networks;
using Microcks.Testcontainers.Connection;

namespace Microcks.Testcontainers;

/// <inheritdoc cref="ContainerBuilder{TBuilderEntity, TContainerEntity, TConfigurationEntity}" />
public sealed class MicrocksAsyncMinionBuilder
    : ContainerBuilder<MicrocksAsyncMinionBuilder, MicrocksAsyncMinionContainer, MicrocksAsyncMinionConfiguration>
{
    public const int MicrocksAsyncMinionHttpPort = 8081;
    private const string MicrocksAsyncMinionFullImageName = "quay.io/microcks/microcks-uber-async-minion";

    private HashSet<string> extraProtocols = [];
    private INetwork _network;

    public MicrocksAsyncMinionBuilder(INetwork network)
        : this(new MicrocksAsyncMinionConfiguration())
    {
        this._network = network;
        DockerResourceConfiguration = Init().DockerResourceConfiguration;
    }

    private MicrocksAsyncMinionBuilder(MicrocksAsyncMinionConfiguration resourceConfiguration)
        : base(resourceConfiguration)
    {
        DockerResourceConfiguration = resourceConfiguration;
    }

    protected override MicrocksAsyncMinionConfiguration DockerResourceConfiguration { get; }

    public override MicrocksAsyncMinionContainer Build()
    {
        Validate();

        return new MicrocksAsyncMinionContainer(DockerResourceConfiguration);
    }

    protected override MicrocksAsyncMinionBuilder Init()
    {
        return base.Init()
            .WithImage(MicrocksAsyncMinionFullImageName)
            .WithNetwork(this._network)
            .WithNetworkAliases("microcks-async-minion")
            .WithEnvironment("MICROCKS_HOST_PORT", "microcks:" + MicrocksBuilder.MicrocksHttpPort)
            .WithExposedPort(MicrocksAsyncMinionHttpPort)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilMessageIsLogged(".*Profile prod activated\\..*"));
    }

    /// <inheritdoc />
    protected override MicrocksAsyncMinionBuilder Clone(IResourceConfiguration<CreateContainerParameters> resourceConfiguration)
    {
        return Merge(DockerResourceConfiguration, new MicrocksAsyncMinionConfiguration(resourceConfiguration));
    }

    /// <inheritdoc />
    protected override MicrocksAsyncMinionBuilder Clone(IContainerConfiguration resourceConfiguration)
    {
        return Merge(DockerResourceConfiguration, new MicrocksAsyncMinionConfiguration(resourceConfiguration));
    }

    /// <inheritdoc />
    protected override MicrocksAsyncMinionBuilder Merge(MicrocksAsyncMinionConfiguration oldValue, MicrocksAsyncMinionConfiguration newValue)
    {
        return new MicrocksAsyncMinionBuilder(new MicrocksAsyncMinionConfiguration(oldValue, newValue));
    }


    /// <summary>
    /// Configures the MicrocksAsyncMinionBuilder to use a Kafka connection.
    /// </summary>
    /// <param name="kafkaConnection">The Kafka connection details.</param>
    /// <returns>The updated MicrocksAsyncMinionBuilder instance.</returns>
    public MicrocksAsyncMinionBuilder WithKafkaConnection(KafkaConnection kafkaConnection)
    {
        extraProtocols.Add("KAFKA");
        var environments = new Dictionary<string, string>
        {
            { "ASYNC_PROTOCOLS", $",{string.Join(",", extraProtocols)}" },
            { "KAFKA_BOOTSTRAP_SERVER", kafkaConnection.BootstrapServers },
        };

        return Merge(DockerResourceConfiguration, new MicrocksAsyncMinionConfiguration(new ContainerConfiguration(environments: environments)));
    }
}
