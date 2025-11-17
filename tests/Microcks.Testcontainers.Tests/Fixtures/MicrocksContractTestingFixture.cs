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
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Networks;

namespace Microcks.Testcontainers.Tests.Fixtures;

public sealed class MicrocksContractTestingFixture : IAsyncLifetime
{
    private static readonly string BAD_PASTRY_IMAGE = "quay.io/microcks/contract-testing-demo:01";
    private static readonly string GOOD_PASTRY_IMAGE = "quay.io/microcks/contract-testing-demo:02";

    public INetwork Network { get; }
    public MicrocksContainer MicrocksContainer { get; }
    public IContainer BadImpl { get; }
    public IContainer GoodImpl { get; }

    public MicrocksContractTestingFixture()
    {
        Network = new NetworkBuilder().Build();

        MicrocksContainer = new MicrocksBuilder()
            .WithNetwork(Network)
            .Build();

        BadImpl = new ContainerBuilder()
            .WithImage(BAD_PASTRY_IMAGE)
            .WithNetwork(Network)
            .WithNetworkAliases("bad-impl")
            .WithWaitStrategy(Wait.ForUnixContainer().UntilMessageIsLogged(".*Example app listening on port 3001.*"))
            .Build();

        GoodImpl = new ContainerBuilder()
            .WithImage(GOOD_PASTRY_IMAGE)
            .WithNetwork(Network)
            .WithNetworkAliases("good-impl")
            .WithWaitStrategy(Wait.ForUnixContainer().UntilMessageIsLogged(".*Example app listening on port 3002.*"))
            .Build();
    }

    public async ValueTask InitializeAsync()
    {
        MicrocksContainer.Started +=
            (_, _) => MicrocksContainer.ImportAsMainArtifact("apipastries-openapi.yaml");

        await Network.CreateAsync();
        await MicrocksContainer.StartAsync();
        await BadImpl.StartAsync();
        await GoodImpl.StartAsync();
    }

    public async ValueTask DisposeAsync()
    {
        // Dispose of the containers in reverse order of creation
        await MicrocksContainer.DisposeAsync();
        await BadImpl.DisposeAsync();
        await GoodImpl.DisposeAsync();
        await Network.DisposeAsync();
    }

}
