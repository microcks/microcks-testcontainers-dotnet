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

using Microcks.Testcontainers;
using NHamcrest;
using System;
using System.Net;
using System.Text.Json;
using Microcks.Testcontainers.Model;
using RestAssured.Logging;
using RestAssured.Response;
using Xunit.Internal;
using Microsoft.AspNetCore.Mvc;

namespace Microcks.Testcontainers.Tests;

public sealed class MicrocksSecretCreationTests : IAsyncLifetime
{
    private readonly MicrocksContainer _microcksContainer = new MicrocksBuilder()
      .WithSecrets(new SecretBuilder().WithName("my-secret").WithToken("abc-123-xyz").WithTokenHeader("x-microcks").Build())
      .WithMainRemoteArtifacts(new RemoteArtifact("https://raw.githubusercontent.com/microcks/microcks/master/samples/APIPastry-openapi.yaml", "my-secret"))
      .Build();

    public async ValueTask DisposeAsync()
    {
        await _microcksContainer.DisposeAsync();
    }

    public async ValueTask InitializeAsync()
    {
        await _microcksContainer.StartAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task ShouldFindSecrets()
    {
        var result = await Given()
          .When()
          .Log(new LogConfiguration { RequestLogLevel = RequestLogLevel.Body })
          .Get(_microcksContainer.GetHttpEndpoint() + "api/secrets")
          .Then()
          .StatusCode(HttpStatusCode.OK)
          .And()
          .Body("$[0].name", Is.EqualTo("my-secret"))
          .And()
          .Body("$[0].token", Is.EqualTo("abc-123-xyz"))
          .And()
          .Body("$[0].tokenHeader", Is.EqualTo("x-microcks"))
          .Extract()
          .Response().Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        var document = JsonDocument.Parse(result);
        Assert.Equal(1, document.RootElement.GetArrayLength());
    }

    [Fact]
    public async Task ShouldLoadRemoteArtifactUsingSecret()
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
          .Response().Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        var document = JsonDocument.Parse(services);
        Assert.Equal(1, document.RootElement.GetArrayLength());

        verifiableResponse.Body("$[0:].name", Has.Items(
          Is.EqualTo("API Pastry - 2.0")
          ),
        VerifyAs.Json);
    }

    [Fact]
    public void ShouldNotThrowExceptionWhenNameDefined()
    {
        var secret = new SecretBuilder()
          .WithName("my-secret")
          .Build();

        Assert.NotNull(secret);
        Assert.Equal("my-secret", secret.Name);
    }

    [Fact]
    public void ShouldThrowExceptionWhenNameNotDefined()
    {
        Assert.Throws<ArgumentNullException>(() => new SecretBuilder().Build());
    }
}
