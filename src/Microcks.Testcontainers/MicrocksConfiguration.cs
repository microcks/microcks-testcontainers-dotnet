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

namespace Microcks.Testcontainers;

/// <inheritdoc cref="ContainerConfiguration" />
public sealed class MicrocksConfiguration : ContainerConfiguration
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MicrocksConfiguration" /> class.
    /// </summary>
    /// <param name="snapshots">The snapshots to import.</param>
    /// <param name="mainArtifacts">The main artifacts to import.</param>
    /// <param name="secondaryArtifacts">The secondary artifacts to import.</param>
    /// <param name="mainRemoteArtifacts">The main remote artifacts to download.</param>
    /// <param name="secondaryRemoteArtifacts">The secondary remote artifacts to download.</param>
    /// <param name="secrets">The secrets to create.</param>
    public MicrocksConfiguration(
        IEnumerable<string> snapshots = null,
        IEnumerable<string> mainArtifacts = null,
        IEnumerable<string> secondaryArtifacts = null,
        IEnumerable<RemoteArtifact> mainRemoteArtifacts = null,
        IEnumerable<RemoteArtifact> secondaryRemoteArtifacts = null,
        IEnumerable<Model.Secret> secrets = null)
    {
        Snapshots = snapshots;
        MainArtifacts = mainArtifacts;
        SecondaryArtifacts = secondaryArtifacts;
        MainRemoteArtifacts = mainRemoteArtifacts;
        SecondaryRemoteArtifacts = secondaryRemoteArtifacts;
        Secrets = secrets;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MicrocksConfiguration" /> class.
    /// </summary>
    /// <param name="resourceConfiguration">The Docker resource configuration.</param>
    public MicrocksConfiguration(IResourceConfiguration<CreateContainerParameters> resourceConfiguration)
        : base(resourceConfiguration)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MicrocksConfiguration" /> class.
    /// </summary>
    /// <param name="resourceConfiguration">The Docker resource configuration.</param>
    public MicrocksConfiguration(IContainerConfiguration resourceConfiguration)
        : base(resourceConfiguration)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MicrocksConfiguration" /> class.
    /// </summary>
    /// <param name="resourceConfiguration">The Docker resource configuration.</param>
    public MicrocksConfiguration(MicrocksConfiguration resourceConfiguration)
        : this(new MicrocksConfiguration(), resourceConfiguration)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MicrocksConfiguration" /> class.
    /// </summary>
    /// <param name="oldValue">The old Docker resource configuration.</param>
    /// <param name="newValue">The new Docker resource configuration.</param>
    public MicrocksConfiguration(MicrocksConfiguration oldValue, MicrocksConfiguration newValue)
        : base(oldValue, newValue)
    {
        Snapshots = BuildConfiguration.Combine(oldValue.Snapshots, newValue.Snapshots);
        MainArtifacts = BuildConfiguration.Combine(oldValue.MainArtifacts, newValue.MainArtifacts);
        SecondaryArtifacts = BuildConfiguration.Combine(oldValue.SecondaryArtifacts, newValue.SecondaryArtifacts);
        MainRemoteArtifacts = BuildConfiguration.Combine(oldValue.MainRemoteArtifacts, newValue.MainRemoteArtifacts);
        SecondaryRemoteArtifacts = BuildConfiguration.Combine(oldValue.SecondaryRemoteArtifacts, newValue.SecondaryRemoteArtifacts);
        Secrets = BuildConfiguration.Combine(oldValue.Secrets, newValue.Secrets);
    }

    /// <summary>
    /// Gets the snapshots to import into the Microcks container.
    /// </summary>
    public IEnumerable<string> Snapshots { get; }

    /// <summary>
    /// Gets the main artifacts to import into the Microcks container.
    /// </summary>
    public IEnumerable<string> MainArtifacts { get; }

    /// <summary>
    /// Gets the secondary artifacts to import into the Microcks container.
    /// </summary>
    public IEnumerable<string> SecondaryArtifacts { get; }

    /// <summary>
    /// Gets the main remote artifacts to download into the Microcks container.
    /// </summary>
    public IEnumerable<RemoteArtifact> MainRemoteArtifacts { get; }

    /// <summary>
    /// Gets the secondary remote artifacts to download into the Microcks container.
    /// </summary>
    public IEnumerable<RemoteArtifact> SecondaryRemoteArtifacts { get; }

    /// <summary>
    /// Gets the secrets to create into the Microcks container.
    /// </summary>
    public IEnumerable<Model.Secret> Secrets { get; }
}
