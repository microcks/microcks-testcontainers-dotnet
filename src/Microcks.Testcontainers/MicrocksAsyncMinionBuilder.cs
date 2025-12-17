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
    /// <summary>
    /// The default HTTP port for the Microcks async minion.
    /// </summary>
    public const ushort MicrocksAsyncMinionHttpPort = 8081;

    private const string MicrocksAsyncMinionFullImageName = "quay.io/microcks/microcks-uber-async-minion";

    private readonly HashSet<string> _extraProtocols = [];
    private readonly INetwork _network;

    /// <inheritdoc />
    protected override MicrocksAsyncMinionConfiguration DockerResourceConfiguration { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MicrocksAsyncMinionBuilder"/> class with the specified network.
    /// </summary>
    /// <param name="network">The network to be used by the Microcks async minion container.</param>
    public MicrocksAsyncMinionBuilder(INetwork network)
        : this(new MicrocksAsyncMinionConfiguration())
    {
        this._network = network;
        DockerResourceConfiguration = Init().DockerResourceConfiguration;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MicrocksAsyncMinionBuilder"/> class with the specified resource configuration.
    /// </summary>
    /// <param name="resourceConfiguration">The resource configuration for the Microcks async minion container.</param>
    private MicrocksAsyncMinionBuilder(MicrocksAsyncMinionConfiguration resourceConfiguration)
        : base(resourceConfiguration)
    {
        DockerResourceConfiguration = resourceConfiguration;
    }

    /// <inheritdoc />
    public override MicrocksAsyncMinionContainer Build()
    {
        Validate();

        return new MicrocksAsyncMinionContainer(DockerResourceConfiguration);
    }

    /// <inheritdoc />
    protected override MicrocksAsyncMinionBuilder Init()
    {
        return base.Init()
            .WithImage(MicrocksAsyncMinionFullImageName)
            .WithNetwork(this._network)
            .WithNetworkAliases("microcks-async-minion")
            .WithEnvironment("MICROCKS_HOST_PORT", "microcks:" + MicrocksBuilder.MicrocksHttpPort)
            .WithPortBinding(MicrocksAsyncMinionHttpPort, true)
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
        _extraProtocols.Add("KAFKA");
        var environments = new Dictionary<string, string>
        {
            { "ASYNC_PROTOCOLS", $",{string.Join(",", _extraProtocols)}" },
            { "KAFKA_BOOTSTRAP_SERVER", kafkaConnection.BootstrapServers },
        };

        return Merge(DockerResourceConfiguration, new MicrocksAsyncMinionConfiguration(new ContainerConfiguration(environments: environments)));
    }

    /// <summary>
    /// Configures the MicrocksAsyncMinionBuilder to use an AMQP connection.
    /// </summary>
    /// <param name="amqpConnection">The AMQP connection details.</param>
    /// <returns>The updated MicrocksAsyncMinionBuilder instance.</returns>
    public MicrocksAsyncMinionBuilder WithAmqpConnection(GenericConnection amqpConnection)
    {
        _extraProtocols.Add("AMQP");
        var environments = new Dictionary<string, string>
        {
            { "ASYNC_PROTOCOLS", $",{string.Join(",", _extraProtocols)}" },
            { "AMQP_SERVER", amqpConnection.Url },
            { "AMQP_USERNAME", amqpConnection.Username },
            { "AMQP_PASSWORD", amqpConnection.Password },
        };

        return Merge(DockerResourceConfiguration, new MicrocksAsyncMinionConfiguration(new ContainerConfiguration(environments: environments)));
    }

    /// <summary>
    /// Environment variable name used by Quarkus to configure console log level.
    /// </summary>
    public const string QuarkusConsoleLogLevelEnvVar = "QUARKUS_LOG_CONSOLE_LEVEL";

    /// <summary>
    /// Environment variable name used by Quarkus to configure the Microcks category log level.
    /// </summary>
    public const string QuarkusMicrocksCategoryLogLevelEnvVar = "QUARKUS_LOG_CATEGORY__IO_GITHUB_MICROCKS__LEVEL";

    /// <summary>
    /// Environment variable value for enabling DEBUG logging.
    /// </summary>
    public const string DebugLogLevel = "DEBUG";

    /// <summary>
    /// Enables DEBUG log level for Microcks async minion components inside the container.
    /// </summary>
    /// <remarks>
    /// This follows Microcks documentation for the Quarkus-based async-minion image.
    /// It must be called before <see cref="Build"/> / container start.
    /// </remarks>
    public MicrocksAsyncMinionBuilder WithDebugLogLevel()
    {
        return this
            .WithEnvironment(QuarkusConsoleLogLevelEnvVar, DebugLogLevel)
            .WithEnvironment(QuarkusMicrocksCategoryLogLevelEnvVar, DebugLogLevel);
    }
}
