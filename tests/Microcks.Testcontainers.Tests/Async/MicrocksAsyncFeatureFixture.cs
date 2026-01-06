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

using System;
using DotNet.Testcontainers;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;

namespace Microcks.Testcontainers.Tests.Async;

[CollectionDefinition(nameof(MicrocksAsyncFeatureCollection))]
public class MicrocksAsyncFeatureCollection : ICollectionFixture<MicrocksAsyncFeatureFixture> { }

/// <summary>
/// Fixture for Microcks Async Feature tests.
/// </summary>
public class MicrocksAsyncFeatureFixture : IAsyncLifetime
{
    /// <summary>
    /// Image name for the Microcks container.
    /// </summary>
    private const string MicrocksImage = "quay.io/microcks/microcks-uber:1.10.1-native";

    private const string BadPastryAsyncImage = "quay.io/microcks/contract-testing-demo-async:01";
    private const string GoodPastryAsyncImage = "quay.io/microcks/contract-testing-demo-async:02";

    public MicrocksContainerEnsemble MicrocksContainerEnsemble { get; private set; }
    private IContainer WsGoodImplContainer { get; set; }
    private IContainer WsBadImplContainer { get; set; }


    public async ValueTask DisposeAsync()
    {
        await this.MicrocksContainerEnsemble.DisposeAsync();
        await this.WsBadImplContainer.DisposeAsync();
        await this.WsGoodImplContainer.DisposeAsync();
        GC.SuppressFinalize(this);
    }

    public async ValueTask InitializeAsync()
    {
        ConsoleLogger.Instance.DebugLogLevelEnabled = true;

        this.MicrocksContainerEnsemble = new MicrocksContainerEnsemble(MicrocksImage)
            .WithMainArtifacts("pastry-orders-asyncapi.yml")
            .WithAsyncFeature();

        this.WsBadImplContainer = new ContainerBuilder(BadPastryAsyncImage)
            .WithNetwork(this.MicrocksContainerEnsemble.Network)
            .WithNetworkAliases("bad-impl")
            .WithExposedPort(4001)
            .WithWaitStrategy(
                Wait.ForUnixContainer()
                    .UntilMessageIsLogged(".*Starting WebSocket server on ws://localhost:4001/websocket.*")
            )
            .Build();

        this.WsGoodImplContainer = new ContainerBuilder(GoodPastryAsyncImage)
            .WithNetwork(this.MicrocksContainerEnsemble.Network)
            .WithNetworkAliases("good-impl")
            .WithExposedPort(4002)
            .WithWaitStrategy(
                Wait.ForUnixContainer()
                    .UntilMessageIsLogged(".*Starting WebSocket server on ws://localhost:4002/websocket.*")
            )
            .Build();

        await this.MicrocksContainerEnsemble.StartAsync(TestContext.Current.CancellationToken);
        await this.WsBadImplContainer.StartAsync(TestContext.Current.CancellationToken);
        await this.WsGoodImplContainer.StartAsync(TestContext.Current.CancellationToken);
    }

}
