# Microcks Testcontainers .NET

.NET library for Testcontainers that enables embedding Microcks into your unit tests with lightweight, throwaway instance thanks to containers.

[![GitHub Workflow Status](https://img.shields.io/github/actions/workflow/status/microcks/microcks-testcontainers-dotnet/cicd.yml?logo=github&style=for-the-badge)](https://github.com/microcks/microcks-testcontainers-dotnet/actions)
[![Version](https://img.shields.io/nuget/v/Microcks.Testcontainers?color=blue&style=for-the-badge)](https://www.nuget.org/packages/Microcks.Testcontainers)
[![License](https://img.shields.io/github/license/microcks/microcks-testcontainers-java?style=for-the-badge&logo=apache)](https://www.apache.org/licenses/LICENSE-2.0)
[![Project Chat](https://img.shields.io/badge/discord-microcks-pink.svg?color=7289da&style=for-the-badge&logo=discord)](https://microcks.io/discord-invite/)
[![Artifact HUB](https://img.shields.io/endpoint?url=https://artifacthub.io/badge/repository/microcks-uber-image&style=for-the-badge)](https://artifacthub.io/packages/search?repo=microcks-uber-image)
[![CNCF Landscape](https://img.shields.io/badge/CNCF%20Landscape-5699C6?style=for-the-badge&logo=cncf)](https://landscape.cncf.io/?item=app-definition-and-development--application-definition-image-build--microcks)

## Build Status

Current development version is `0.1.0`.

#### Sonarcloud Quality metrics

[![Code Smells](https://sonarcloud.io/api/project_badges/measure?project=microcks_microcks-testcontainers-dotnet&metric=code_smells)](https://sonarcloud.io/summary/new_code?id=microcks_microcks-testcontainers-dotnet)
[![Reliability Rating](https://sonarcloud.io/api/project_badges/measure?project=microcks_microcks-testcontainers-dotnet&metric=reliability_rating)](https://sonarcloud.io/summary/new_code?id=microcks_microcks-testcontainers-dotnet)
[![Bugs](https://sonarcloud.io/api/project_badges/measure?project=microcks_microcks-testcontainers-dotnet&metric=bugs)](https://sonarcloud.io/summary/new_code?id=microcks_microcks-testcontainers-dotnet)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=microcks_microcks-testcontainers-dotnet&metric=coverage)](https://sonarcloud.io/summary/new_code?id=microcks_microcks-testcontainers-dotnet)
[![Technical Debt](https://sonarcloud.io/api/project_badges/measure?project=microcks_microcks-testcontainers-dotnet&metric=sqale_index)](https://sonarcloud.io/summary/new_code?id=microcks_microcks-testcontainers-dotnet)
[![Security Rating](https://sonarcloud.io/api/project_badges/measure?project=microcks_microcks-testcontainers-dotnet&metric=security_rating)](https://sonarcloud.io/summary/new_code?id=microcks_microcks)
[![Maintainability Rating](https://sonarcloud.io/api/project_badges/measure?project=microcks_microcks-testcontainers-dotnet&metric=sqale_rating)](https://sonarcloud.io/summary/new_code?id=microcks_microcks-testcontainers-dotnet)

#### Fossa license and security scans

[![FOSSA Status](https://app.fossa.com/api/projects/git%2Bgithub.com%2Fmicrocks%2Fmicrocks-testcontainers-dotnet.svg?type=shield&issueType=license)](https://app.fossa.com/projects/git%2Bgithub.com%2Fmicrocks%2Fmicrocks-testcontainers-dotnet?ref=badge_shield&issueType=license)
[![FOSSA Status](https://app.fossa.com/api/projects/git%2Bgithub.com%2Fmicrocks%2Fmicrocks-testcontainers-dotnet.svg?type=shield&issueType=security)](https://app.fossa.com/projects/git%2Bgithub.com%2Fmicrocks%2Fmicrocks-testcontainers-dotnet?ref=badge_shield&issueType=security)
[![FOSSA Status](https://app.fossa.com/api/projects/git%2Bgithub.com%2Fmicrocks%2Fmicrocks-testcontainers-dotnet.svg?type=small)](https://app.fossa.com/projects/git%2Bgithub.com%2Fmicrocks%2Fmicrocks-testcontainers-dotnet?ref=badge_small)

#### OpenSSF best practices on Microcks core

[![CII Best Practices](https://bestpractices.coreinfrastructure.org/projects/7513/badge)](https://bestpractices.coreinfrastructure.org/projects/7513)
[![OpenSSF Scorecard](https://api.securityscorecards.dev/projects/github.com/microcks/microcks/badge)](https://securityscorecards.dev/viewer/?uri=github.com/microcks/microcks)

## Community

* [Documentation](https://microcks.io/documentation/tutorials/getting-started/)
* [Microcks Community](https://github.com/microcks/community) and community meeting
* Join us on [Discord](https://microcks.io/discord-invite/), on [GitHub Discussions](https://github.com/orgs/microcks/discussions) or [CNCF Slack #microcks channel](https://cloud-native.slack.com/archives/C05BYHW1TNJ)

To get involved with our community, please make sure you are familiar with the project's [Code of Conduct](./CODE_OF_CONDUCT.md).

## How to use it?

### Include it into your project dependencies

```
dotnet add package Microcks.Testcontainers --version 0.1.0
```

### Startup the container

You just have to specify the container image you'd like to use. This library requires a Microcks `uber` distribution (with no MongoDB dependency).


```csharp
MicrocksContainer container = new MicrocksBuilder()
	.WithImage("quay.io/microcks/microcks-uber:1.10.0")
	.Build();
await container.StartAsync();
```

### Import content in Microcks

To use Microcks mocks or contract-testing features, you first need to import OpenAPI, Postman Collection, GraphQL or gRPC artifacts. 
Artifacts can be imported as main/Primary ones or as secondary ones. See [Multi-artifacts support](https://microcks.io/documentation/using/importers/#multi-artifacts-support) for details.

You can do it before starting the container using simple paths:

```csharp
MicrocksContainer container = new MicrocksBuilder()
	  .WithMainArtifacts("apipastries-openapi.yaml")
	  .WithSecondaryArtifacts("apipastries-postman-collection.json")
	  .Build();
await container.StartAsync();
```

or once the container started using `File` arguments:

```csharp
container.ImportAsMainArtifact("apipastries-openapi.yaml");
container.ImportAsSecondaryArtifact("apipastries-postman-collection.json");
```

You can also import full [repository snapshots](https://microcks.io/documentation/administrating/snapshots/) at once:

```csharp
MicrocksContainer container = new MicrocksBuilder()
      .WithSnapshots("microcks-repository.json")
	  .Build();
await container.StartAsync();
```

### Using mock endpoints for your dependencies

During your test setup, you'd probably need to retrieve mock endpoints provided by Microcks containers to 
setup your base API url calls. You can do it like this:

```csharp
var baseApiUrl = container.GetRestMockEndpoint("API Pastries", "0.0.1");
```

The container provides methods for different supported API styles/protocols (Soap, GraphQL, gRPC,...).

The container also provides `GetHttpEndpoint()` for raw access to those API endpoints.

### Launching new contract-tests

If you want to ensure that your application under test is conformant to an OpenAPI contract (or other type of contract),
you can launch a Microcks contract/conformance test using the local server port you're actually running.

```csharp
private int port;

public async Task InitializeAsync()
{
	container = new MicrocksBuilder()
		.WithExposedPort(port)
		.Build();
	await container.StartAsync();
}

[Fact]
public async Task testOpenAPIContract()
{
	var testRequest = new TestRequest
	{
		ServiceId = "API Pastries:0.0.1",
		RunnerType = TestRunnerType.OPEN_API_SCHEMA,
		TestEndpoint = $"http://host.testcontainers.internal:{port}",
		Timeout = TimeSpan.FromMilliseconds(2000)
	};
	TestResult testResult = await container.TestEndpointAsync(testRequest);

	testResult.Success.Should().BeTrue();
}
```

The `TestResult` gives you access to all details regarding success of failure on different test cases.
