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

using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics.CodeAnalysis;
using DotNet.Testcontainers.Networks;
using Microcks.Testcontainers.Connection;
using Microcks.Testcontainers.Helpers;

namespace Microcks.Testcontainers;

/// <summary>
/// Represents a container ensemble for Microcks,
/// including the main Microcks container and an optional asynchronous minion container.
/// </summary>
public class MicrocksContainerEnsemble : IAsyncDisposable, IArtifactAndSnapshotManager<MicrocksContainerEnsemble>
{
    private MicrocksBuilder _microcksBuilder;

    private ContainerBuilder _postmanBuilder;
    private MicrocksAsyncMinionBuilder _asyncMinionBuilder;

    private bool _debugLogLevelEnabled;

    /// <summary>
    /// Gets the Postman runtime container.
    /// </summary>
    public IContainer PostmanContainer { get; private set; }

    /// <summary>
    /// Gets the Microcks asynchronous minion container.
    /// </summary>
    public MicrocksAsyncMinionContainer AsyncMinionContainer { get; private set; }

    /// <summary>
    /// Gets the Microcks container.
    /// </summary>
    public MicrocksContainer MicrocksContainer { get; private set; }

    private readonly INetwork _network;

    /// <summary>
    /// Gets the Docker network used by this ensemble.
    /// </summary>
    public INetwork Network { get => this._network; }


    private readonly string _microcksImage;

    /// <summary>
    /// Initializes a new instance of the <see cref="MicrocksContainerEnsemble"/> class.
    /// </summary>
    /// <param name="microcksImage">The name of the Microcks image to be used.</param>
    public MicrocksContainerEnsemble(string microcksImage)
        : this(new NetworkBuilder().Build(), microcksImage)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MicrocksContainerEnsemble"/> class.
    /// </summary>
    /// <param name="network">The network to be used by the Microcks container ensemble.</param>
    /// <param name="microcksImage">The name of the Microcks image to be used.</param>
    [SuppressMessage("Security", "S5332", Justification = "HTTP is used intentionally for container-to-container communication in unit/integration tests on localhost")]
    public MicrocksContainerEnsemble(INetwork network, string microcksImage)
    {
        this._microcksImage = microcksImage;
        this._network = network;

        this._microcksBuilder = new MicrocksBuilder(this._microcksImage)
            .WithNetwork(this._network)
            .WithNetworkAliases("microcks")
            .WithExposedPort(MicrocksBuilder.MicrocksHttpPort)
            .WithExposedPort(MicrocksBuilder.MicrocksGrpcPort)
            .WithEnvironment(MacOSHelper.GetJavaOptions())
            .WithEnvironment("POSTMAN_RUNNER_URL", "http://postman:3000")
            .WithEnvironment("TEST_CALLBACK_URL", "http://microcks:" + MicrocksBuilder.MicrocksHttpPort)
            .WithEnvironment("ASYNC_MINION_URL", "http://microcks-async-minion:" + MicrocksAsyncMinionBuilder.MicrocksAsyncMinionHttpPort);
    }

    /// <summary>
    /// Configures the Microcks container ensemble with the specified main artifacts.
    /// </summary>
    /// <param name="mainArtifacts">An array of main artifact file names to be used by the Microcks container.</param>
    /// <returns>The updated <see cref="MicrocksContainerEnsemble"/> instance.</returns>
    public MicrocksContainerEnsemble WithMainArtifacts(params string[] mainArtifacts)
    {
        this._microcksBuilder.WithMainArtifacts(mainArtifacts);
        return this;
    }

    /// <summary>
    /// Configures the Microcks container ensemble with the specified main remote artifacts.
    /// </summary>
    /// <param name="mainRemoteArtifacts">An array of remote artifact URIs/locations to be used by the Microcks container.</param>
    /// <returns>The updated <see cref="MicrocksContainerEnsemble"/> instance.</returns>
    public MicrocksContainerEnsemble WithMainRemoteArtifacts(params string[] mainRemoteArtifacts)
    {
        this._microcksBuilder.WithMainRemoteArtifacts(mainRemoteArtifacts);
        return this;
    }

    /// <summary>
    /// Configures the Microcks container ensemble with the specified main remote artifacts.
    /// </summary>
    /// <param name="mainRemoteArtifacts">The remote artifact definitions to be used by the Microcks container.</param>
    /// <returns>The updated <see cref="MicrocksContainerEnsemble"/> instance.</returns>
    public MicrocksContainerEnsemble WithMainRemoteArtifacts(params RemoteArtifact[] mainRemoteArtifacts)
    {
        this._microcksBuilder.WithMainRemoteArtifacts(mainRemoteArtifacts);
        return this;
    }

    /// <summary>
    /// Configures the Microcks container ensemble with the specified secondary artifacts.
    /// </summary>
    /// <param name="secondaryArtifacts">An array of secondary artifact file names to be used by the Microcks container.</param>
    /// <returns>The updated <see cref="MicrocksContainerEnsemble"/> instance.</returns>
    public MicrocksContainerEnsemble WithSecondaryArtifacts(params string[] secondaryArtifacts)
    {
        this._microcksBuilder.WithSecondaryArtifacts(secondaryArtifacts);
        return this;
    }

    /// <summary>
    /// Configures the Microcks container ensemble with the specified secondary remote artifacts.
    /// </summary>
    /// <param name="secondaryRemoteArtifacts">The remote artifact definitions to be used by the Microcks container.</param>
    /// <returns>The updated <see cref="MicrocksContainerEnsemble"/> instance.</returns>
    public MicrocksContainerEnsemble WithSecondaryRemoteArtifacts(params RemoteArtifact[] secondaryRemoteArtifacts)
    {
        this._microcksBuilder.WithSecondaryRemoteArtifacts(secondaryRemoteArtifacts);
        return this;
    }

    /// <summary>
    /// Configures the Microcks container ensemble with the specified snapshots.
    /// </summary>
    /// <param name="snapshots">An array of snapshot file names to be loaded into the Microcks container.</param>
    /// <returns>The updated <see cref="MicrocksContainerEnsemble"/> instance.</returns>
    public MicrocksContainerEnsemble WithSnapshots(params string[] snapshots)
    {
        this._microcksBuilder.WithSnapshots(snapshots);
        return this;
    }

    /// <summary>
    /// Configures the Microcks container ensemble with the specified secrets.
    /// </summary>
    /// <param name="secrets">The secrets to create in the Microcks container.</param>
    /// <returns>The updated <see cref="MicrocksContainerEnsemble"/> instance.</returns>
    public MicrocksContainerEnsemble WithSecrets(params Model.Secret[] secrets)
    {
        this._microcksBuilder.WithSecrets(secrets);
        return this;
    }

    /// <summary>
    /// Configures the Microcks container ensemble to include a Postman runtime container.
    /// </summary>
    /// <returns>The updated <see cref="MicrocksContainerEnsemble"/> instance.</returns>
    public MicrocksContainerEnsemble WithPostman()
    {
        return this.WithPostman("quay.io/microcks/microcks-postman-runtime:latest");
    }

    /// <summary>
    /// Configures the Microcks container ensemble to include a Postman runtime container with the specified image
    /// </summary>
    /// <param name="image">The Postman runtime image to be used.</param>
    /// <returns>The updated <see cref="MicrocksContainerEnsemble"/> instance.</returns>
    public MicrocksContainerEnsemble WithPostman(string image)
    {
        this._postmanBuilder = new ContainerBuilder(image)
            .WithNetwork(this._network)
            .WithNetworkAliases("postman")
            .WithWaitStrategy(Wait.ForUnixContainer().UntilMessageIsLogged(".*postman-runtime wrapper listening on port.*"));
        return this;
    }

    /// <summary>
    /// Configures the Microcks container ensemble to use the asynchronous feature.
    /// </summary>
    /// <returns>
    /// The <see cref="MicrocksContainerEnsemble"/> instance with the asynchronous feature configured.
    /// </returns>
    /// <remarks>
    /// This method modifies the Microcks image to use the asynchronous version by replacing "microcks-uber" with "microcks-uber-async".
    /// If the image name ends with "-native", it removes the "-native" suffix.
    /// It also sets up the asynchronous minion builder with the modified image, network, and network aliases.
    /// </remarks>
    public MicrocksContainerEnsemble WithAsyncFeature()
    {
        if (this._asyncMinionBuilder != null)
        {
            return this;
        }

        var image = this._microcksImage.Replace("microcks-uber", "microcks-uber-async-minion");
        if (image.EndsWith("-native"))
        {
            image = image.Replace("-native", "");
        }

        this._asyncMinionBuilder = new MicrocksAsyncMinionBuilder(this._network)
            .WithEnvironment(MacOSHelper.GetJavaOptions())
            .WithImage(image);

        if (this._debugLogLevelEnabled)
        {
            this._asyncMinionBuilder = this._asyncMinionBuilder.WithDebugLogLevel();
        }

        return this;
    }

    /// <summary>
    /// Configures the Microcks container ensemble with a Kafka connection.
    /// </summary>
    /// <param name="kafkaConnection">The Kafka connection details.</param>
    /// <returns>The updated <see cref="MicrocksContainerEnsemble"/> instance.</returns>
    public MicrocksContainerEnsemble WithKafkaConnection(KafkaConnection kafkaConnection)
    {
        // Ensure the asynchronous feature is enabled.
        this.WithAsyncFeature();

        this._asyncMinionBuilder = (_asyncMinionBuilder ?? throw new InvalidOperationException("MicrocksAsyncMinionBuilder is null"))
            .WithKafkaConnection(kafkaConnection);

        return this;
    }

    /// <summary>
    /// Configures the Microcks container ensemble with an AMQP connection.
    /// </summary>
    /// <param name="amqpConnection">The AMQP connection details.</param>
    /// <returns>The updated <see cref="MicrocksContainerEnsemble"/> instance.</returns>
    public MicrocksContainerEnsemble WithAmqpConnection(GenericConnection amqpConnection)
    {
        // Ensure the asynchronous feature is enabled.
        this.WithAsyncFeature();

        this._asyncMinionBuilder = (_asyncMinionBuilder ?? throw new InvalidOperationException("MicrocksAsyncMinionBuilder is null"))
            .WithAmqpConnection(amqpConnection);

        return this;
    }

    /// <summary>
    /// Enables DEBUG log level for the containers of this ensemble.
    /// </summary>
    /// <remarks>
    /// Applies to the main Microcks container (Spring) and, when the async feature is enabled,
    /// to the async-minion container (Quarkus).
    /// </remarks>
    public MicrocksContainerEnsemble WithDebugLogLevel()
    {
        this._debugLogLevelEnabled = true;
        this._microcksBuilder = this._microcksBuilder.WithDebugLogLevel();

        // If async feature has already been enabled, apply immediately.
        this._asyncMinionBuilder = this._asyncMinionBuilder?.WithDebugLogLevel();

        return this;
    }

    /// <summary>
    /// Starts the Microcks container ensemble asynchronously.
    /// </summary>
    /// <param name="cancellationToken">A CancellationToken to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous start operation.</returns>
    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        this.MicrocksContainer = this._microcksBuilder.Build();
        await this.MicrocksContainer.StartAsync(cancellationToken);

        if (this._postmanBuilder != null)
        {
            this.PostmanContainer = this._postmanBuilder
                .Build();

            await this.PostmanContainer.StartAsync(cancellationToken);
        }
        if (this._asyncMinionBuilder != null)
        {
            this.AsyncMinionContainer = this._asyncMinionBuilder
                .Build();

            await this.AsyncMinionContainer.StartAsync(cancellationToken);
        }
    }

    /// <summary>
    /// Disposes the Microcks container ensemble asynchronously.
    /// </summary>
    /// <returns>A task that represents the asynchronous dispose operation.</returns>
    public async ValueTask DisposeAsync()
    {
        if (this.PostmanContainer != null)
        {
            await this.PostmanContainer.DisposeAsync();
        }
        if (this.AsyncMinionContainer != null)
        {
            await this.AsyncMinionContainer.DisposeAsync();
        }

        if (this.MicrocksContainer != null)
        {
            await this.MicrocksContainer.DisposeAsync();
        }
        
        GC.SuppressFinalize(this);
    }
}
