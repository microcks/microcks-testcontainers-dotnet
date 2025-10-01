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

/// <summary>
/// Artifacts management interface for fluent approach.
/// </summary>
public interface IArtifactAndSnapshotManager<T>
{
    /// <summary>
    /// Add main/primary artifacts.
    /// </summary>
    T WithMainArtifacts(params string[] mainArtifacts);

    /// <summary>
    /// Add main/primary remote artifacts.
    /// </summary>
    T WithMainRemoteArtifacts(params string[] mainRemoteArtifacts);

    /// <summary>
    /// Add main/primary remote artifacts with a complete full definition of remote artifact.
    /// </summary>
    T WithMainRemoteArtifacts(params RemoteArtifact[] mainRemoteArtifacts);

    /// <summary>
    /// Add secondary artifacts.
    /// </summary>
    T WithSecondaryArtifacts(params string[] secondaryArtifacts);

    /// <summary>
    /// Add secondary artifacts with a complete full definition of remote artifact.
    /// </summary>
    T WithSecondaryRemoteArtifacts(params RemoteArtifact[] secondaryRemoteArtifacts);

    /// <summary>
    /// Add snapshots.
    /// </summary>
    T WithSnapshots(params string[] snapshots);
}
