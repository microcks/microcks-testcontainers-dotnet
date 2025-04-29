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
using DotNet.Testcontainers.Networks;
using Microcks.Testcontainers;
using Microcks.Testcontainers.Model;
using System;
using System.Collections.Generic;
using System.Net;

namespace Microcks.Testcontainers.Tests;

public sealed class MicrocksContractTestingFunctionalityTests : IAsyncLifetime
{
    private readonly INetwork _network = new NetworkBuilder().Build();
    private readonly MicrocksContainer _microcksContainer;
    private readonly IContainer _badImpl;
    private readonly IContainer _goodImpl;


    private static readonly string BAD_PASTRY_IMAGE = "quay.io/microcks/contract-testing-demo:01";
    private static readonly string GOOD_PASTRY_IMAGE = "quay.io/microcks/contract-testing-demo:02";

    public MicrocksContractTestingFunctionalityTests()
    {
        _microcksContainer = new MicrocksBuilder()
            .WithNetwork(_network)
            .Build();

        _badImpl = new ContainerBuilder()
          .WithImage(BAD_PASTRY_IMAGE)
          .WithNetwork(_network)
          .WithNetworkAliases("bad-impl")
          .WithWaitStrategy(Wait.ForUnixContainer().UntilMessageIsLogged(".*Example app listening on port 3001.*"))
          .Build();

        _goodImpl = new ContainerBuilder()
          .WithImage(GOOD_PASTRY_IMAGE)
          .WithNetwork(_network)
          .WithNetworkAliases("good-impl")
          .WithWaitStrategy(Wait.ForUnixContainer().UntilMessageIsLogged(".*Example app listening on port 3002.*"))
          .Build();
    }

    public Task DisposeAsync()
    {
        return Task.WhenAll(
          _microcksContainer.DisposeAsync().AsTask(),
          _badImpl.DisposeAsync().AsTask(),
          _goodImpl.DisposeAsync().AsTask(),
          _network.DisposeAsync().AsTask()
        );
    }

    public Task InitializeAsync()
    {
        _microcksContainer.Started +=
          (_, _) => _microcksContainer.ImportAsMainArtifact("apipastries-openapi.yaml");

        return Task.WhenAll(
          _network.CreateAsync(),
          _microcksContainer.StartAsync(),
          _badImpl.StartAsync(),
          _goodImpl.StartAsync()
        );
    }

    [Fact]
    public void ShouldConfigRetrieval()
    {
        var uriBuilder = new UriBuilder(_microcksContainer.GetHttpEndpoint())
        {
            Path = "/api/keycloak/config"
        };

        Given()
          .When()
          .Get(uriBuilder.ToString())
          .Then()
          .StatusCode(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ShouldReturnSuccess_WhenGoodImplementation()
    {
        var badTestRequest = new TestRequest
        {
            ServiceId = "API Pastries:0.0.1",
            RunnerType = TestRunnerType.OPEN_API_SCHEMA,
            TestEndpoint = "http://bad-impl:3001",
            Timeout = TimeSpan.FromMilliseconds(2000)
        };

        // First test should fail with validation failure messages.
        TestResult badTestResult = await _microcksContainer.TestEndpointAsync(badTestRequest);
        Assert.False(badTestResult.Success);
        Assert.Equal("http://bad-impl:3001", badTestResult.TestedEndpoint);
        Assert.Equal(3, badTestResult.TestCaseResults.Count);
        Assert.Contains("string found, number expected", badTestResult.TestCaseResults[0].TestStepResults[0].Message);

        // Retrieve messages for the failing test case.
        List<RequestResponsePair> messages = await _microcksContainer.GetMessagesForTestCaseAsync(badTestResult, "GET /pastries");
        Assert.Equal(3, messages.Count);
        for (int i = 0; i < messages.Count; i++)
        {
            Assert.NotNull(messages[i].Request);
            Assert.NotNull(messages[i].Response);
            Assert.NotNull(messages[i].Response.Content);
            // Check these are the correct requests.
            Assert.NotNull(messages[i].Request.QueryParameters);
            Assert.Single(messages[i].Request.QueryParameters);
            Assert.Equal("size", messages[i].Request.QueryParameters[0].Name);
        }

        // Switch endpoint to good implementation
        var goodTestRequest = new TestRequest
        {
            ServiceId = "API Pastries:0.0.1",
            RunnerType = TestRunnerType.OPEN_API_SCHEMA,
            TestEndpoint = "http://good-impl:3002",
            Timeout = TimeSpan.FromMilliseconds(2000)
        };
        TestResult goodTestResult = await _microcksContainer.TestEndpointAsync(goodTestRequest);
        Assert.True(goodTestResult.Success);
        Assert.Equal("http://good-impl:3002", goodTestResult.TestedEndpoint);
        Assert.Equal(3, goodTestResult.TestCaseResults.Count);
        Assert.Empty(goodTestResult.TestCaseResults[0].TestStepResults[0].Message);

        // Test avec un header
        var goodTestRequestWithHeader = new TestRequest
        {
            ServiceId = "API Pastries:0.0.1",
            RunnerType = TestRunnerType.OPEN_API_SCHEMA,
            TestEndpoint = "http://good-impl:3002",
            Timeout = TimeSpan.FromSeconds(2),
            OperationsHeaders = new Dictionary<string, List<Header>>()
              {
                {
                  "GET /pastries",
                  new List<Header>
                  {
                    new() {
                      Name = "X-Custom-Header-1",
                      Values = "value1,value2,value3"
                    }
                  }
                }
              }
        };

        TestResult goodTestResultWithHeader = await _microcksContainer.TestEndpointAsync(goodTestRequestWithHeader);
        Assert.True(goodTestResultWithHeader.Success);
        Assert.Equal("http://good-impl:3002", goodTestResultWithHeader.TestedEndpoint);
        Assert.Equal(3, goodTestResultWithHeader.TestCaseResults.Count);
        Assert.Empty(goodTestResultWithHeader.TestCaseResults[0].TestStepResults[0].Message);
        Assert.Single(goodTestResultWithHeader.OperationsHeaders);
        Assert.True(goodTestResultWithHeader.OperationsHeaders.ContainsKey("GET /pastries"));
        Assert.Single(goodTestResultWithHeader.OperationsHeaders["GET /pastries"]);
        var header = goodTestResultWithHeader.OperationsHeaders["GET /pastries"][0];
        Assert.Equal("X-Custom-Header-1", header.Name);

        var actualItems = header.Values.Split(",");
        var expectedItems = new[] { "value1", "value2", "value3" };

        foreach (var expectedItem in expectedItems)
        {
            Assert.Contains(expectedItem, actualItems);
        }
    }
}
