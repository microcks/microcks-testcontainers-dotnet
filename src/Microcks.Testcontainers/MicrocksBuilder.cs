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
    /// Image name for the Microcks container.
    /// </summary>
    public const string MicrocksImage = "quay.io/microcks/microcks-uber";

    /// <summary>
    /// HTTP port for the Microcks container.
    /// </summary>
    public const ushort MicrocksHttpPort = 8080;

    /// <summary>
    /// GRPC port for the Microcks container.
    /// </summary>
    public const ushort MicrocksGrpcPort = 9090;

    private List<string> _snapshots;

    private List<string> _mainRemoteArtifacts;
    private List<string> _mainArtifacts;
    private List<string> _secondaryArtifacts;

    private List<Model.Secret> _secrets;

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
        if (_snapshots != null && _snapshots.Any())
        {
            _snapshots.ForEach(snapshot => container.ImportSnapshotAsync(snapshot).GetAwaiter().GetResult());
        }

        if (_mainRemoteArtifacts != null && _mainRemoteArtifacts.Any())
        {
            _mainRemoteArtifacts.ForEach(remoteArtifactUrl =>
                container.DownloadArtifactAsync(remoteArtifactUrl, main: true).GetAwaiter().GetResult());
        }

        if (_mainArtifacts != null && _mainArtifacts.Any())
        {
            _mainArtifacts.ForEach(container.ImportAsMainArtifact);
        }

        if (_secondaryArtifacts != null && _secondaryArtifacts.Any())
        {
            _secondaryArtifacts.ForEach(container.ImportAsSecondaryArtifact);
        }

        if (_secrets != null && _secrets.Any())
        {
            _secrets.ForEach(secret => container.CreateSecretAsync(secret).GetAwaiter().GetResult());
        }
    }


    /// <inheritdoc />
    protected override MicrocksBuilder Init()
    {
        return base.Init()
            .WithEnvironment(MacOSHelper.GetJavaOptions())
            .WithImage(MicrocksImage)
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
        if (_snapshots == null)
        {
            _snapshots = new List<string>(snapshots);
        }
        else
        {
            _snapshots.AddRange(snapshots);
        }

        return this;
    }

    /// <summary>
    /// Set the main remote artifacts to download into the Microcks container.
    /// </summary>
    /// <param name="urls"></param>
    /// <returns></returns>
    public MicrocksBuilder WithMainRemoteArtifacts(params string[] urls)
    {
        if (_mainRemoteArtifacts == null)
        {
            _mainRemoteArtifacts = new List<string>(urls);
        }
        else
        {
            _mainRemoteArtifacts.AddRange(urls);
        }

        return this;
    }

    /// <summary>
    /// Set the main artifacts to import into the Microcks container.
    /// </summary>
    /// <param name="mainArtifacts"></param>
    /// <returns></returns>
    public MicrocksBuilder WithMainArtifacts(params string[] mainArtifacts)
    {
        if (_mainArtifacts == null)
        {
            _mainArtifacts = new List<string>(mainArtifacts);
        }
        else
        {
            _mainArtifacts.AddRange(mainArtifacts);
        }

        return this;
    }

    /// <summary>
    /// Set the secondary artifacts to import into the Microcks container.
    /// </summary>
    /// <param name="secondaryArtifacts"></param>
    /// <returns></returns>
    public MicrocksBuilder WithSecondaryArtifacts(params string[] secondaryArtifacts)
    {
        if (_secondaryArtifacts == null)
        {
            _secondaryArtifacts = new List<string>(secondaryArtifacts);
        }
        else
        {
            _secondaryArtifacts.AddRange(secondaryArtifacts);
        }

        return this;
    }

    /// <summary>
    /// Set the secrets to create into the Microcks container.
    /// </summary>
    /// <param name="secrets"></param>
    /// <returns></returns>
    public MicrocksBuilder WithSecret(params Model.Secret[] secrets)
    {
        if (_secrets == null)
        {
            _secrets = new List<Model.Secret>(secrets);
        }
        else
        {
            _secrets.AddRange(secrets);
        }

        return this;
    }
}
