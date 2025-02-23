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

using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;

using Microcks.Testcontainers.Model;
using System;
using System.Diagnostics;

namespace Microcks.Testcontainers.Tests;

/// <summary>
/// This class contains tests for the WaitForConditionAsync method.
/// </summary>
public class WaitForConditionAsyncTests : IAsyncLifetime
{
    /// <summary>
    /// Image name for the Microcks container.
    /// </summary>
    private const string MicrocksImage = "quay.io/microcks/microcks-uber:1.10.1-native";

    private const string GoodPastryAsyncImage = "quay.io/microcks/contract-testing-demo-async:02";

    private MicrocksContainerEnsemble _microcksContainerEnsemble;
    private IContainer _wsGoodImplContainer;

    public async Task InitializeAsync()
    {
        this._microcksContainerEnsemble = new MicrocksContainerEnsemble(MicrocksImage)
            .WithMainArtifacts("pastry-orders-asyncapi.yml")
            .WithAsyncFeature();

        this._wsGoodImplContainer = new ContainerBuilder()
            .WithImage(GoodPastryAsyncImage)
            .WithNetwork(this._microcksContainerEnsemble.Network)
            .WithNetworkAliases("good-impl")
            .WithExposedPort(4002)
            .WithWaitStrategy(
                Wait.ForUnixContainer()
                    .UntilMessageIsLogged(".*Starting WebSocket server on ws://localhost:4002/websocket.*")
            )
            .Build();

        await this._microcksContainerEnsemble.StartAsync();
        await this._wsGoodImplContainer.StartAsync();

    }

    public async Task DisposeAsync()
    {
        await this._microcksContainerEnsemble.DisposeAsync();
        await this._wsGoodImplContainer.DisposeAsync();
    }

    /// <summary>
    /// Test that verifies that the WaitForConditionAsync method throws a TaskCanceledException
    /// when the specified timeout is reached.
    /// </summary>
    [Fact]
    public async Task WaitForConditionAsyncShouldThrowTaskCanceledExceptionWhenTimeoutIsReached()
    {
        // New Test request
        var testRequest = new TestRequest
        {
            ServiceId = "Pastry orders API:0.1.0",
            RunnerType = TestRunnerType.ASYNC_API_SCHEMA,
            Timeout = TimeSpan.FromMilliseconds(200),
            TestEndpoint = "ws://good-impl:4002/websocket",
        };

        var taskTestResult = _microcksContainerEnsemble.MicrocksContainer
            .TestEndpointAsync(testRequest);

        var stopwatch = new Stopwatch();
        stopwatch.Start();
        var testResult = await taskTestResult;
        stopwatch.Stop();

        // Assert
        Assert.True(stopwatch.ElapsedMilliseconds > 200);
        Assert.True(testResult.InProgress);
        Assert.False(testResult.Success);
        Assert.Equal(testRequest.TestEndpoint, testResult.TestedEndpoint);
    }
}

