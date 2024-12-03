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
public sealed class MicrocksAsyncMinionConfiguration : ContainerConfiguration
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MicrocksAsyncMinionConfiguration" /> class.
    /// </summary>
    /// <param name="config">The Microcks config.</param>
    public MicrocksAsyncMinionConfiguration(object config = null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MicrocksAsyncMinionConfiguration" /> class.
    /// </summary>
    /// <param name="resourceConfiguration">The Docker resource configuration.</param>
    public MicrocksAsyncMinionConfiguration(IResourceConfiguration<CreateContainerParameters> resourceConfiguration)
        : base(resourceConfiguration)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MicrocksAsyncMinionConfiguration" /> class.
    /// </summary>
    /// <param name="resourceConfiguration">The Docker resource configuration.</param>
    public MicrocksAsyncMinionConfiguration(IContainerConfiguration resourceConfiguration)
        : base(resourceConfiguration)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MicrocksAsyncMinionConfiguration" /> class.
    /// </summary>
    /// <param name="resourceConfiguration">The Docker resource configuration.</param>
    public MicrocksAsyncMinionConfiguration(MicrocksAsyncMinionConfiguration resourceConfiguration)
        : this(new MicrocksAsyncMinionConfiguration(), resourceConfiguration)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MicrocksAsyncMinionConfiguration" /> class.
    /// </summary>
    /// <param name="oldValue">The old Docker resource configuration.</param>
    /// <param name="newValue">The new Docker resource configuration.</param>
    public MicrocksAsyncMinionConfiguration(MicrocksAsyncMinionConfiguration oldValue, MicrocksAsyncMinionConfiguration newValue)
        : base(oldValue, newValue)
    {
    }
}
