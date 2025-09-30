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
using System.IO;
using System.Net;
using System.Text.Json;

using RestAssured.Logging;
using RestAssured.Response;
using NHamcrest;

namespace Microcks.Testcontainers.Tests;

public sealed class MicrocksEnsembleArtifactsTest : IAsyncLifetime
{
    private readonly MicrocksContainerEnsemble _ensemble = new MicrocksContainerEnsemble("quay.io/microcks/microcks-uber")
        .WithSnapshots("microcks-repository.json")
        .WithMainArtifacts("apipastries-openapi.yaml", Path.Combine("subdir", "weather-forecast-openapi.yaml"))
        .WithMainRemoteArtifacts("https://raw.githubusercontent.com/microcks/microcks/master/samples/APIPastry-openapi.yaml")
        .WithSecondaryArtifacts("apipastries-postman-collection.json");

    public async ValueTask DisposeAsync()
    {
        await _ensemble.DisposeAsync();
    }

    public async ValueTask InitializeAsync()
    {
        await _ensemble.StartAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task ShouldAvailableServices()
    {
        var uriBuilder = new UriBuilder(_ensemble.MicrocksContainer.GetHttpEndpoint())
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
          .Response().Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

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

}
