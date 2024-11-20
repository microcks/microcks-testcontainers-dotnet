# microcks-testcontainers-dotnet
[![FOSSA Status](https://app.fossa.com/api/projects/git%2Bgithub.com%2Fmicrocks%2Fmicrocks-testcontainers-dotnet.svg?type=shield)](https://app.fossa.com/projects/git%2Bgithub.com%2Fmicrocks%2Fmicrocks-testcontainers-dotnet?ref=badge_shield)

.NET lib for Testcontainers that enables embedding Microcks into your unit tests with lightweight, throwaway instance thanks to containers.


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


## License
[![FOSSA Status](https://app.fossa.com/api/projects/git%2Bgithub.com%2Fmicrocks%2Fmicrocks-testcontainers-dotnet.svg?type=large)](https://app.fossa.com/projects/git%2Bgithub.com%2Fmicrocks%2Fmicrocks-testcontainers-dotnet?ref=badge_large)