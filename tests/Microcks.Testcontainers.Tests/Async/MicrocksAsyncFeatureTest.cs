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

using FluentAssertions;

namespace Microcks.Testcontainers.Tests.Async;

public sealed class MicrocksAsyncFeatureTest : IAsyncLifetime
{
    /// <summary>
    /// Image name for the Microcks container.
    /// </summary>
    private const string MicrocksImage = "quay.io/microcks/microcks-uber:1.10.1-native";

    private MicrocksContainerEnsemble _microcksContainerEnsemble;

    public async Task DisposeAsync()
    {
        await this._microcksContainerEnsemble.DisposeAsync();
    }

    public async Task InitializeAsync()
    {
        this._microcksContainerEnsemble = new MicrocksContainerEnsemble(MicrocksImage)
            .WithAsyncFeature();

        await this._microcksContainerEnsemble.StartAsync();
    }

    [Fact]
    public void ShouldDetermineCorrectImageMessage()
    {
        this._microcksContainerEnsemble.AsyncMinionContainer.Image.FullName
            .Should()
            .Be("quay.io/microcks/microcks-uber-async-minion:1.10.1");
    }

}
