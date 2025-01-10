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

using FluentAssertions;
using Microcks.Testcontainers;
using NHamcrest;
using System;
using System.Net;
using System.Text.Json;
using Microcks.Testcontainers.Model;
using RestAssured.Logging;

namespace Testcontainers.Microcks.Tests;

public sealed class MicrocksSecretCreationTests : IAsyncLifetime
{
    private readonly MicrocksContainer _microcksContainer = new MicrocksBuilder()
      .WithSecret(new SecretBuilder().WithName("my-secret").WithToken("abc-123-xyz").Build())
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
          .Extract()
          .Response().Content.ReadAsStringAsync();

        var document = JsonDocument.Parse(result);
        document.RootElement.EnumerateArray().Should().HaveCount(1);
    }

    [Fact]
    public void ShouldNotThrowExceptionWhenNameDefined()
    {
        var secret = new SecretBuilder()
          .WithName("my-secret")
          .Build();
        secret.Should().NotBeNull();
        secret.Name.Should().Be("my-secret");
    }

    [Fact]
    public void ShouldThrowExceptionWhenNameNotDefined()
    {
        Assert.Throws<ArgumentNullException>(() => new SecretBuilder().Build());
    }
}
