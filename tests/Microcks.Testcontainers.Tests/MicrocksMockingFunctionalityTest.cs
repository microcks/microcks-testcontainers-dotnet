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

using NHamcrest;
using NHamcrest.Core;
using RestAssured.Logging;
using RestAssured.Response;
using System;
using System.IO;
using System.Net;
using System.Text.Json;

namespace Microcks.Testcontainers.Tests;

public sealed class MicrocksMockingFunctionalityTest : IAsyncLifetime
{
    private readonly MicrocksContainer _microcksContainer = new MicrocksBuilder()
      .WithSnapshots("microcks-repository.json")
      .WithMainArtifacts("apipastries-openapi.yaml", Path.Combine("subdir", "weather-forecast-openapi.yaml"))
      .WithMainRemoteArtifacts("https://raw.githubusercontent.com/microcks/microcks/master/samples/APIPastry-openapi.yaml")
      .WithSecondaryArtifacts("apipastries-postman-collection.json")
      .Build();

    public Task DisposeAsync()
    {
        return _microcksContainer.DisposeAsync().AsTask();
    }

    public Task InitializeAsync()
    {
        return _microcksContainer.StartAsync();
    }

    [Fact]
    public void ShouldConfigureMockEndpoints()
    {
        string baseWsUrl = $"{_microcksContainer.GetSoapMockEndpoint("Pastries Service", "1.0")}";
        Assert.Equal($"{_microcksContainer.GetHttpEndpoint()}soap/Pastries Service/1.0", baseWsUrl);

        string baseApiUrl = $"{_microcksContainer.GetRestMockEndpoint("API Pastries", "0.0.1")}";
        Assert.Equal($"{_microcksContainer.GetHttpEndpoint()}rest/API Pastries/0.0.1", baseApiUrl);

        string baseGraphUrl = $"{_microcksContainer.GetGraphQLMockEndpoint("Pastries Graph", "1")}";
        Assert.Equal($"{_microcksContainer.GetHttpEndpoint()}graphql/Pastries Graph/1", baseGraphUrl);

        string baseGrpcUrl = $"{_microcksContainer.GetGrpcMockEndpoint()}";
        Assert.Equal($"grpc://{_microcksContainer.Hostname}:{_microcksContainer.GetMappedPublicPort(MicrocksBuilder.MicrocksGrpcPort)}/", baseGrpcUrl);
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
    public async Task ShouldAvailableServices()
    {
        var uriBuilder = new UriBuilder(_microcksContainer.GetHttpEndpoint())
        {
            Path = "/api/services"
        };

        var verifiableResponse = Given()
          .Log(new LogConfiguration { RequestLogLevel = RequestLogLevel.All })
          .When()
          .Get(uriBuilder.ToString())
          .Then()
          .StatusCode(HttpStatusCode.OK);

        // newtonsoft json jsonpath $.length is not supported
        var services = await verifiableResponse
          .Extract()
          .Response().Content.ReadAsStringAsync();

        var document = JsonDocument.Parse(services);
        Assert.Equal(7, document.RootElement.GetArrayLength());

        verifiableResponse.Body("$[0:].name", Has.Items(
            Is.EqualTo("Petstore API"),
            Is.EqualTo("HelloService Mock"),
            Is.EqualTo("io.github.microcks.grpc.hello.v1.HelloService"),
            Is.EqualTo("Movie Graph API"),
            Is.EqualTo("API Pastry - 2.0"),
            Is.EqualTo("API Pastries"),
            Is.EqualTo("WeatherForecast API")
            ),
          VerifyAs.Json);
    }

    [Fact]
    public void ShouldMockVariousCapabilities()
    {
        var pastries = _microcksContainer.GetRestMockEndpoint("API Pastries", "0.0.1");

        Given()
          .When()
          .Get($"{pastries}/pastries/Millefeuille")
          .Then()
          .StatusCode(HttpStatusCode.OK)
          .Body("$.name", IsEqualMatcher<string>.EqualTo("Millefeuille"));


        // Vérifier que le mock de l'API Pastry est bien disponible
        Given()
          .When()
          .Get($"{pastries}/pastries/Eclair Chocolat")
          .Then()
          .StatusCode(HttpStatusCode.OK)
          .Body("$.name", IsEqualMatcher<string>.EqualTo("Eclair Chocolat"));

        var baseApiUrl = _microcksContainer.GetRestMockEndpoint("API Pastry - 2.0", "2.0.0");

        Given()
          .When()
          .Get($"{baseApiUrl}" + "/pastry/Millefeuille")
          .Then()
          .StatusCode(HttpStatusCode.OK)
          .Body("$.name", IsEqualMatcher<string>.EqualTo("Millefeuille"));
    }

}
