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

using Microcks.Testcontainers.Helpers;
using Microcks.Testcontainers.Model;
using Microcks.Testcontainers.Tests.Fixtures;
using System;
using System.Net;
using Testcontainers.Keycloak;
using TestResult = Microcks.Testcontainers.Model.TestResult;

namespace Microcks.Testcontainers.Tests;

[Collection("MicrocksTests")]
public sealed class MicrocksContractTestingFunctionalityWithOAuth2Tests
    : IAsyncLifetime
{
    private readonly MicrocksContractTestingFixture _fixture;

    private readonly KeycloakContainer _keycloak;

    public MicrocksContractTestingFunctionalityWithOAuth2Tests(MicrocksContractTestingFixture fixture)
    {
        _fixture = fixture;
        var network = _fixture.Network;
        _keycloak = new KeycloakBuilder("quay.io/keycloak/keycloak:26.0.0")
            .WithEnvironment(MacOSHelper.GetJavaOptions("JAVA_OPTS_APPEND"))
            .WithNetwork(network)
            .WithNetworkAliases("keycloak")
            .WithCommand("--import-realm")
            .WithResourceMapping("./myrealm-realm.json", "/opt/keycloak/data/import")
            .Build();
    }

    public async ValueTask DisposeAsync()
    {
        await _keycloak.DisposeAsync();
    }

    public async ValueTask InitializeAsync()
    {
        await _keycloak.StartAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public void ShouldConfigRetrieval()
    {
        var uriBuilder = new UriBuilder(_fixture.MicrocksContainer.GetHttpEndpoint())
        {
            Path = "/api/keycloak/config"
        };

        var response = Given()
            .When()
            .Get(uriBuilder.ToString())
            .Then()
            .StatusCode(HttpStatusCode.OK)
            .Extract()
            .Response();

        Assert.NotNull(response);
        Assert.NotEmpty(response.Content.Headers.ContentType?.ToString());
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

        TestResult testResult = await _fixture.MicrocksContainer.TestEndpointAsync(testRequest, TestContext.Current.CancellationToken);
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
