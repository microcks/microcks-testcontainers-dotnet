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
/// Interface pour la gestion des artifacts et snapshots dans une approche fluide.
/// </summary>
public interface IArtifactAndSnapshotManager<T>
{
    /// <summary>
    /// Ajoute des artifacts principaux.
    /// </summary>
    T WithMainArtifacts(params string[] mainArtifacts);

    /// <summary>
    /// Ajoute des artifacts principaux distants.
    /// </summary>
    T WithMainRemoteArtifacts(params string[] mainRemoteArtifacts);

    /// <summary>
    /// Ajoute des artifacts secondaires.
    /// </summary>
    T WithSecondaryArtifacts(params string[] secondaryArtifacts);

    /// <summary>
    /// Ajoute des snapshots.
    /// </summary>
    T WithSnapshots(params string[] snapshots);
}
