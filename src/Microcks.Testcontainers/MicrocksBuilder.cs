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

using Microcks.Testcontainers.Helpers;

namespace Microcks.Testcontainers;

/// <inheritdoc cref="ContainerBuilder{TBuilderEntity, TContainerEntity, TConfigurationEntity}" />
public sealed class MicrocksBuilder : ContainerBuilder<MicrocksBuilder, MicrocksContainer, MicrocksConfiguration>,
    IArtifactAndSnapshotManager<MicrocksBuilder>
{
    /// <summary>
    /// Default image name for the Microcks container.
    /// </summary>
    public const string MicrocksImage = "quay.io/microcks/microcks-uber";

    /// <summary>
    /// Image name for the Microcks container.
    /// </summary>
    private readonly string _microcksImage = MicrocksImage;

    /// <summary>
    /// HTTP port for the Microcks container.
    /// </summary>
    public const ushort MicrocksHttpPort = 8080;

    /// <summary>
    /// GRPC port for the Microcks container.
    /// </summary>
    public const ushort MicrocksGrpcPort = 9090;

    /// <summary>
    /// Initializes a new instance of the <see cref="MicrocksBuilder" /> class.
    /// </summary>
    /// <param name="image">The Docker image to use.</param>
    public MicrocksBuilder(string image)
        : this(new MicrocksConfiguration())
    {
        _microcksImage = image;
        DockerResourceConfiguration = Init().DockerResourceConfiguration;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MicrocksBuilder" /> class.
    /// </summary>
    public MicrocksBuilder()
        : this(new MicrocksConfiguration())
    {
        DockerResourceConfiguration = Init().DockerResourceConfiguration;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MicrocksBuilder" /> class.
    /// </summary>
    /// <param name="resourceConfiguration">The Docker resource configuration.</param>
    private MicrocksBuilder(MicrocksConfiguration resourceConfiguration)
        : base(resourceConfiguration)
    {
        DockerResourceConfiguration = resourceConfiguration;
    }

    /// <inheritdoc />
    protected override MicrocksConfiguration DockerResourceConfiguration { get; }

    /// <inheritdoc />
    public override MicrocksContainer Build()
    {
        Validate();
        var container = new MicrocksContainer(DockerResourceConfiguration);

        container.Started += (_, _) => ContainerStarted(container);

        return container;
    }

    private void ContainerStarted(MicrocksContainer container)
    {
        var configuration = DockerResourceConfiguration;

        // Load snapshots before anything else.
        if (configuration.Snapshots != null && configuration.Snapshots.Any())
        {
            foreach (var snapshot in configuration.Snapshots)
            {
                container.ImportSnapshotAsync(snapshot).GetAwaiter().GetResult();
            }
        }
        // Load secrets before remote artifacts as they may be needed for authentication.
        if (configuration.Secrets != null && configuration.Secrets.Any())
        {
            foreach (var secret in configuration.Secrets)
            {
                container.CreateSecretAsync(secret).GetAwaiter().GetResult();
            }
        }
        // Load remote artifacts before local ones.
        if (configuration.MainRemoteArtifacts != null && configuration.MainRemoteArtifacts.Any())
        {
            foreach (var remoteArtifact in configuration.MainRemoteArtifacts)
            {
                container.DownloadArtifactAsync(remoteArtifact, main: true).GetAwaiter().GetResult();
            }
        }
        if (configuration.SecondaryRemoteArtifacts != null && configuration.SecondaryRemoteArtifacts.Any())
        {
            foreach (var remoteArtifact in configuration.SecondaryRemoteArtifacts)
            {
                container.DownloadArtifactAsync(remoteArtifact, main: false).GetAwaiter().GetResult();
            }
        }

        if (configuration.MainArtifacts != null && configuration.MainArtifacts.Any())
        {
            foreach (var artifact in configuration.MainArtifacts)
            {
                container.ImportAsMainArtifact(artifact);
            }
        }

        if (configuration.SecondaryArtifacts != null && configuration.SecondaryArtifacts.Any())
        {
            foreach (var artifact in configuration.SecondaryArtifacts)
            {
                container.ImportAsSecondaryArtifact(artifact);
            }
        }
    }


    /// <inheritdoc />
    protected override MicrocksBuilder Init()
    {
        return base.Init()
            .WithImage(_microcksImage)
            .WithPortBinding(MicrocksHttpPort, true)
            .WithPortBinding(MicrocksGrpcPort, true)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilMessageIsLogged(".*Started MicrocksApplication.*"));
    }

    /// <inheritdoc />
    protected override MicrocksBuilder Clone(IResourceConfiguration<CreateContainerParameters> resourceConfiguration)
    {
        return Merge(DockerResourceConfiguration, new MicrocksConfiguration(resourceConfiguration));
    }

    /// <inheritdoc />
    protected override MicrocksBuilder Clone(IContainerConfiguration resourceConfiguration)
    {
        return Merge(DockerResourceConfiguration, new MicrocksConfiguration(resourceConfiguration));
    }

    /// <inheritdoc />
    protected override MicrocksBuilder Merge(MicrocksConfiguration oldValue, MicrocksConfiguration newValue)
    {
        return new MicrocksBuilder(new MicrocksConfiguration(oldValue, newValue));
    }

    /// <summary>
    /// Set the snapshots to import into the Microcks container.
    /// </summary>
    /// <param name="snapshots"></param>
    /// <returns></returns>
    public MicrocksBuilder WithSnapshots(params string[] snapshots)
    {
        return Merge(DockerResourceConfiguration, new MicrocksConfiguration(snapshots: snapshots));
    }

    /// <summary>
    /// Set the main remote artifacts to download into the Microcks container.
    /// </summary>
    /// <param name="mainRemoteArtifacts"></param>
    /// <returns></returns>
    public MicrocksBuilder WithMainRemoteArtifacts(params RemoteArtifact[] mainRemoteArtifacts)
    {
        return Merge(DockerResourceConfiguration, new MicrocksConfiguration(mainRemoteArtifacts: mainRemoteArtifacts));
    }

    /// <summary>
    /// Set the main remote artifacts to download into the Microcks container.
    /// </summary>
    /// <param name="mainRemoteArtifacts"></param>
    /// <returns></returns>
    public MicrocksBuilder WithMainRemoteArtifacts(params string[] mainRemoteArtifacts)
    {
        return Merge(DockerResourceConfiguration, new MicrocksConfiguration(
            mainRemoteArtifacts: mainRemoteArtifacts.Select(url => new RemoteArtifact(url, null))));
    }

    /// <summary>
    /// Set the main artifacts to import into the Microcks container.
    /// </summary>
    /// <param name="mainArtifacts"></param>
    /// <returns></returns>
    public MicrocksBuilder WithMainArtifacts(params string[] mainArtifacts)
    {
        return Merge(DockerResourceConfiguration, new MicrocksConfiguration(mainArtifacts: mainArtifacts));
    }

    /// <summary>
    /// Set the secondary remote artifacts to download into the Microcks container.
    /// </summary>
    /// <param name="secondaryRemoteArtifacts"></param>
    /// <returns></returns>
    public MicrocksBuilder WithSecondaryRemoteArtifacts(params RemoteArtifact[] secondaryRemoteArtifacts)
    {
        return Merge(DockerResourceConfiguration, new MicrocksConfiguration(secondaryRemoteArtifacts: secondaryRemoteArtifacts));
    }

    /// <summary>
    /// Set the secondary artifacts to import into the Microcks container.
    /// </summary>
    /// <param name="secondaryArtifacts"></param>
    /// <returns></returns>
    public MicrocksBuilder WithSecondaryArtifacts(params string[] secondaryArtifacts)
    {
        return Merge(DockerResourceConfiguration, new MicrocksConfiguration(secondaryArtifacts: secondaryArtifacts));
    }

    /// <summary>
    /// Set the secrets to create into the Microcks container.
    /// </summary>
    /// <param name="secrets"></param>
    /// <returns></returns>
    public MicrocksBuilder WithSecrets(params Model.Secret[] secrets)
    {
        return Merge(DockerResourceConfiguration, new MicrocksConfiguration(secrets: secrets));
    }

    /// <summary>
    /// Enables DEBUG log level for Microcks components inside the container.
    /// </summary>
    public MicrocksBuilder WithDebugLogLevel()
    {
        return this.WithEnvironment(ConfigurationConstants.MicrocksLoggingLevelEnvVar, ConfigurationConstants.DebugLogLevelEnvVar);
    }
}
