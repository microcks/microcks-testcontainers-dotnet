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
using Microcks.Testcontainers.Model;
using TestResult = Microcks.Testcontainers.Model.TestResult;

namespace Microcks.Testcontainers.Tests;

public sealed class PostmanContractTestingFunctionalityTests : IAsyncLifetime
{
    private static readonly string BAD_PASTRY_IMAGE = "quay.io/microcks/contract-testing-demo:02";
    private static readonly string GOOD_PASTRY_IMAGE = "quay.io/microcks/contract-testing-demo:03";

    private readonly INetwork _network = new NetworkBuilder().Build();

    private readonly IContainer _badImpl;
    private readonly IContainer _goodImpl;
    private readonly MicrocksContainerEnsemble _ensemble;


    public PostmanContractTestingFunctionalityTests()
    {
        _badImpl = new ContainerBuilder()
            .WithImage(BAD_PASTRY_IMAGE)
            .WithNetwork(_network)
            .WithNetworkAliases("bad-impl")
            .WithWaitStrategy(Wait.ForUnixContainer().UntilMessageIsLogged(".*Example app listening on port 3002.*"))
            .Build();

        _goodImpl = new ContainerBuilder()
            .WithImage(GOOD_PASTRY_IMAGE)
            .WithNetwork(_network)
            .WithNetworkAliases("good-impl")
            .WithWaitStrategy(Wait.ForUnixContainer().UntilMessageIsLogged(".*Example app listening on port 3003.*"))
            .Build();

        _ensemble = new MicrocksContainerEnsemble(_network, "quay.io/microcks/microcks-uber:1.12.1")
            .WithMainArtifacts("apipastries-openapi.yaml")
            .WithSecondaryArtifacts("apipastries-postman-collection.json")
            .WithPostman();
    }

    public async ValueTask InitializeAsync()
    {
        await _ensemble.StartAsync(TestContext.Current.CancellationToken);
        await _badImpl.StartAsync(TestContext.Current.CancellationToken);
        await _goodImpl.StartAsync(TestContext.Current.CancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        await _badImpl.DisposeAsync();
        await _goodImpl.DisposeAsync();
        await _ensemble.DisposeAsync();
    }

    [Fact]
    public async Task ShouldFail_WhenBadImplementation()
    {
        var testRequest = new TestRequest
        {
            ServiceId = "API Pastries:0.0.1",
            RunnerType = TestRunnerType.POSTMAN,
            TestEndpoint = "http://bad-impl:3002",
            Timeout = TimeSpan.FromSeconds(3)
        };

        // Test should fail with validation failure messages.
        TestResult badTestResult = await _ensemble.MicrocksContainer.TestEndpointAsync(testRequest, TestContext.Current.CancellationToken);
        Assert.False(badTestResult.Success);
        Assert.Equal("http://bad-impl:3002", badTestResult.TestedEndpoint);
        Assert.Equal(3, badTestResult.TestCaseResults.Count);
        // Postman runner stop at first failure so there's just 1 testStepResult per testCaseResult
        Assert.Single(badTestResult.TestCaseResults[0].TestStepResults);
        // Order is not deterministic so it could be a matter of invalid size, invalid name or invalid price.
        Assert.True(badTestResult.TestCaseResults[0].TestStepResults[0].Message.Contains("Valid size in response pastries")
            || badTestResult.TestCaseResults[0].TestStepResults[0].Message.Contains("Valid name in response pastry")
            || badTestResult.TestCaseResults[0].TestStepResults[0].Message.Contains("Valid price in response pastry"));
    }

    [Fact]
    public async Task ShouldSucceed_WhenGoodImplementation()
    {
        var testRequest = new TestRequest
        {
            ServiceId = "API Pastries:0.0.1",
            RunnerType = TestRunnerType.POSTMAN,
            TestEndpoint = "http://good-impl:3003",
            Timeout = TimeSpan.FromSeconds(3)
        };

        // Test should succeed with no validation failure messages.
        TestResult goodTestResult = await _ensemble.MicrocksContainer.TestEndpointAsync(testRequest, TestContext.Current.CancellationToken);
        Assert.True(goodTestResult.Success);
        Assert.Equal("http://good-impl:3003", goodTestResult.TestedEndpoint);
        Assert.Equal(3, goodTestResult.TestCaseResults.Count);
        Assert.Null(goodTestResult.TestCaseResults[0].TestStepResults[0].Message);
    }
}