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
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using FluentAssertions;
using Microcks.Testcontainers.Model;

namespace Microcks.Testcontainers.Tests.Async;

public sealed class MicrocksAsyncFeatureTest : IAsyncLifetime
{
    /// <summary>
    /// Image name for the Microcks container.
    /// </summary>
    private const string MicrocksImage = "quay.io/microcks/microcks-uber:1.10.1-native";

    private const string BadPastryAsyncImage = "quay.io/microcks/contract-testing-demo-async:01";
    private const string GoodPastryAsyncImage = "quay.io/microcks/contract-testing-demo-async:02";

    private MicrocksContainerEnsemble _microcksContainerEnsemble;
    private IContainer _wsGoodImplContainer;
    private IContainer _wsBadImplContainer;

    public async Task DisposeAsync()
    {
        await this._microcksContainerEnsemble.DisposeAsync();
        await this._wsBadImplContainer.DisposeAsync();
        await this._wsGoodImplContainer.DisposeAsync();
    }

    public async Task InitializeAsync()
    {
        this._microcksContainerEnsemble = new MicrocksContainerEnsemble(MicrocksImage)
            .WithMainArtifacts("pastry-orders-asyncapi.yml")
            .WithAsyncFeature();

        this._wsBadImplContainer = new ContainerBuilder()
            .WithImage(BadPastryAsyncImage)
            .WithNetwork(this._microcksContainerEnsemble.Network)
            .WithNetworkAliases("bad-impl")
            .WithExposedPort(4001)
            .WithWaitStrategy(
                Wait.ForUnixContainer()
                    .UntilMessageIsLogged(".*Starting WebSocket server on ws://localhost:4001/websocket.*")
            )
            .Build();

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
        await this._wsBadImplContainer.StartAsync();
        await this._wsGoodImplContainer.StartAsync();
    }

    [Fact]
    public void ShouldDetermineCorrectImageMessage()
    {
        this._microcksContainerEnsemble.AsyncMinionContainer.Image.FullName
            .Should()
            .Be("quay.io/microcks/microcks-uber-async-minion:1.10.1");
    }

    /// <summary>
    /// Test method to verify that a WebSocket message is received when a message is emitted.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task ShouldReceivedWebSocketMessageWhenMessageIsEmitted()
    {
        // Get the WebSocket endpoint for the "Pastry orders API" with version "0.1.0" and subscription "SUBSCRIBE pastry/orders".
        var webSocketEndpoint = _microcksContainerEnsemble
            .AsyncMinionContainer
            .GetWebSocketMockEndpoint("Pastry orders API" ,"0.1.0", "SUBSCRIBE pastry/orders");
        const string expectedMessage = "{\"id\":\"4dab240d-7847-4e25-8ef3-1530687650c8\",\"customerId\":\"fe1088b3-9f30-4dc1-a93d-7b74f0a072b9\",\"status\":\"VALIDATED\",\"productQuantities\":[{\"quantity\":2,\"pastryName\":\"Croissant\"},{\"quantity\":1,\"pastryName\":\"Millefeuille\"}]}";

        using var webSocketClient = new ClientWebSocket();
        await webSocketClient.ConnectAsync(webSocketEndpoint, CancellationToken.None);

        var buffer = new byte[1024];

        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(7));
        var result = await webSocketClient.ReceiveAsync(new ArraySegment<byte>(buffer), cts.Token);
        var message = Encoding.UTF8.GetString(buffer, 0, result.Count);

        await webSocketClient.CloseAsync(
            WebSocketCloseStatus.NormalClosure,
            "Test done",
            CancellationToken.None);

        message.Should().Be(expectedMessage);
    }

    /// <summary>
    /// Test that verifies the correct status contract when a bad message is emitted.
    /// </summary>
    [Fact]
    public async Task ShouldReturnsCorrectStatusContractWhenBadMessageIsEmitted()
    {
        // New Test request
        var testRequest = new TestRequest
        {
            ServiceId = "Pastry orders API:0.1.0",
            RunnerType = TestRunnerType.ASYNC_API_SCHEMA,
            Timeout = TimeSpan.FromMilliseconds(70000),
            TestEndpoint = "ws://bad-impl:4001/websocket",
        };

        var taskTestResult = _microcksContainerEnsemble.MicrocksContainer
            .TestEndpointAsync(testRequest);

        var testResult = await taskTestResult;

        // Assert
        testResult.InProgress.Should().Be(false);
        testResult.Success.Should().Be(false);
        testResult.TestedEndpoint.Should().Be(testRequest.TestEndpoint);

        testResult.TestCaseResults.First().TestStepResults.Should().NotBeEmpty();
        var testStepResult = testResult.TestCaseResults.First().TestStepResults.First();
        testStepResult.Message.Should().Contain("object has missing required properties ([\"status\"]");
    }

    /// <summary>
    /// Test that verifies the correct status contract when a good message is emitted.
    /// </summary>
    [Fact]
    public async Task ShouldReturnsCorrectStatusContractWhenGoodMessageIsEmitted()
    {
        // New Test request
        var testRequest = new TestRequest
        {
            ServiceId = "Pastry orders API:0.1.0",
            RunnerType = TestRunnerType.ASYNC_API_SCHEMA,
            Timeout = TimeSpan.FromMilliseconds(7000),
            TestEndpoint = "ws://good-impl:4002/websocket",
        };

        var taskTestResult = _microcksContainerEnsemble.MicrocksContainer
            .TestEndpointAsync(testRequest);

        var testResult = await taskTestResult;

        // Assert
        testResult.InProgress.Should().Be(false);
        testResult.Success.Should().Be(true);
        testResult.TestedEndpoint.Should().Be(testRequest.TestEndpoint);

        testResult.TestCaseResults.First().TestStepResults.Should().NotBeEmpty();
        testResult.TestCaseResults.First().TestStepResults.First().Message.Should().BeNullOrEmpty();
    }
}
