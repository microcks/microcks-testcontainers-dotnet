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

using System;
using DotNet.Testcontainers.Builders;

namespace Microcks.Testcontainers.Tests;

public sealed class MicrocksContainerEnsembleDisposalTests
{
    private const string MicrocksImage = "quay.io/microcks/microcks-uber";

    [Fact]
    public async Task ShouldDisposeInternallyCreatedNetwork()
    {
        var ensemble = new MicrocksContainerEnsemble(MicrocksImage);
        var network = ensemble.Network;

        await network.CreateAsync(TestContext.Current.CancellationToken);

        try
        {
            await ensemble.DisposeAsync();

            Assert.Throws<InvalidOperationException>(() => _ = network.Name);
        }
        finally
        {
            await network.DisposeAsync();
        }
    }

    [Fact]
    public async Task ShouldPreserveCallerOwnedNetwork()
    {
        var network = new NetworkBuilder().Build();
        await network.CreateAsync(TestContext.Current.CancellationToken);
        var networkName = network.Name;

        try
        {
            var ensemble = new MicrocksContainerEnsemble(network, MicrocksImage);

            await ensemble.DisposeAsync();

            Assert.Equal(networkName, network.Name);
        }
        finally
        {
            await network.DisposeAsync();
        }
    }
}
