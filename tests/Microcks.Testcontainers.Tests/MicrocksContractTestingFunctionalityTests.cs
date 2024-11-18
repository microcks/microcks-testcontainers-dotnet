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
using FluentAssertions;
using Microcks.Testcontainers;
using Microcks.Testcontainers.Model;
using System;
using System.Collections.Generic;
using System.Net;

namespace Testcontainers.Microcks.Tests;

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
        badTestResult.Success.Should().BeFalse();
        badTestResult.TestedEndpoint.Should().Be("http://bad-impl:3001");
        badTestResult.TestCaseResults.Should().HaveCount(3);
        badTestResult.TestCaseResults[0].TestStepResults[0].Message.Should().Contain("object has missing required properties");

        // Switch endpoint to good implementation
        var goodTestRequest = new TestRequest
        {
            ServiceId = "API Pastries:0.0.1",
            RunnerType = TestRunnerType.OPEN_API_SCHEMA,
            TestEndpoint = "http://good-impl:3002",
            Timeout = TimeSpan.FromMilliseconds(2000)
        };
        TestResult goodTestResult = await _microcksContainer.TestEndpointAsync(goodTestRequest);
        goodTestResult.Success.Should().BeTrue();
        goodTestResult.TestedEndpoint.Should().Be("http://good-impl:3002");
        goodTestResult.TestCaseResults.Should().HaveCount(3);
        goodTestResult.TestCaseResults[0].TestStepResults[0].Message.Should().BeEmpty();

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
        goodTestResultWithHeader.Success.Should().BeTrue();
        goodTestResultWithHeader.TestedEndpoint.Should().Be("http://good-impl:3002");
        goodTestResultWithHeader.TestCaseResults.Should().HaveCount(3);
        goodTestResultWithHeader.TestCaseResults[0].TestStepResults[0].Message.Should().BeEmpty();
        goodTestResultWithHeader.OperationsHeaders.Should().HaveCount(1);
        goodTestResultWithHeader.OperationsHeaders.Should().ContainKey("GET /pastries");
        goodTestResultWithHeader.OperationsHeaders["GET /pastries"].Should().HaveCount(1);
        var header = goodTestResultWithHeader.OperationsHeaders["GET /pastries"][0];
        header.Name.Should().Be("X-Custom-Header-1");
        header.Values.Split(",").Should().BeEquivalentTo(["value1", "value2", "value3"]);
    }
}
