//
// Copyright The Microcks Authors.
//
// Licensed under the Apache License, Version 2.0 (the "License")
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
using Microcks.Testcontainers.Helpers;
using System;
using System.Net;
using Testcontainers.Keycloak;

namespace Testcontainers.Microcks.Tests;

public sealed class MicrocksContractTestingFunctionalityWithOAuth2Tests : IAsyncLifetime
{
    private readonly INetwork _network = new NetworkBuilder().Build();
    private readonly MicrocksContainer _microcksContainer;
    private readonly IContainer _goodImpl;
    private readonly KeycloakContainer _keycloak;

    private static readonly string GOOD_PASTRY_IMAGE = "quay.io/microcks/contract-testing-demo:02";

    public MicrocksContractTestingFunctionalityWithOAuth2Tests()
    {
        _microcksContainer = new MicrocksBuilder()
            .WithNetwork(_network)
            .Build();

        _goodImpl = new ContainerBuilder()
            .WithImage(GOOD_PASTRY_IMAGE)
            .WithNetwork(_network)
            .WithNetworkAliases("good-impl")
            .WithWaitStrategy(Wait.ForUnixContainer().UntilMessageIsLogged(".*Example app listening on port 3002.*"))
            .Build();

        _keycloak = new KeycloakBuilder()
            .WithImage("quay.io/keycloak/keycloak:26.0.0")
            .WithEnvironment(MacOSHelper.GetJavaOptions("JAVA_OPTS_APPEND"))
            .WithNetwork(_network)
            .WithNetworkAliases("keycloak")
            .WithCommand("--import-realm")
            .WithResourceMapping("./myrealm-realm.json", "/opt/keycloak/data/import")
            .Build();
    }

    public Task DisposeAsync()
    {
        return Task.WhenAll(
            _microcksContainer.DisposeAsync().AsTask(),
            _keycloak.DisposeAsync().AsTask(),
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
            _keycloak.StartAsync(),
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
        // Switch endpoint to good implementation
        var testRequest = new TestRequest
        {
            ServiceId = "API Pastries:0.0.1",
            RunnerType = TestRunnerType.OPEN_API_SCHEMA,
            TestEndpoint = "http://good-impl:3002",
            Timeout = TimeSpan.FromMilliseconds(2000),
            oAuth2Context = new OAuth2ClientContextBuilder()
                .WithClientId("myrealm-serviceaccount")
                .WithClientSecret("ab54d329-e435-41ae-a900-ec6b3fe15c54")
                .WithTokenUri("http://keycloak:8080/realms/myrealm/protocol/openid-connect/token")
                .WithGrantType(OAuth2GrantType.CLIENT_CREDENTIALS)
                .Build()
        };
        TestResult testResult = await _microcksContainer.TestEndpointAsync(testRequest);
        Assert.True(testResult.Success);
        Assert.Equal("http://good-impl:3002", testResult.TestedEndpoint);
        Assert.Equal(3, testResult.TestCaseResults.Count);
        Assert.Empty(testResult.TestCaseResults[0].TestStepResults[0].Message);

        // Ensure test has used a valid OAuth2 client
        Assert.NotNull(testResult.AuthorizedClient);
        Assert.Equal("myrealm-serviceaccount", testResult.AuthorizedClient.PrincipalName);
        Assert.Equal("http://keycloak:8080/realms/myrealm/protocol/openid-connect/token", testResult.AuthorizedClient.TokenUri);
        Assert.Equal("openid profile email", testResult.AuthorizedClient.Scopes);
    }
}
