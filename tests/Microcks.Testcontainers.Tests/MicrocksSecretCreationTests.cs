using FluentAssertions;
using Microcks.Testcontainers;
using NHamcrest;
using System;
using System.Net;
using System.Text.Json;
using Microcks.Testcontainers.Model;

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
          .Log(RestAssured.Request.Logging.RequestLogLevel.Body)
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
