# Microcks Testcontainers .NET

.NET library for Testcontainers that enables embedding Microcks into your unit tests with lightweight, throwaway instance thanks to containers.

[![GitHub Workflow Status](https://img.shields.io/github/actions/workflow/status/microcks/microcks-testcontainers-dotnet/cicd.yml?logo=github&style=for-the-badge)](https://github.com/microcks/microcks-testcontainers-dotnet/actions)
[![Version](https://img.shields.io/nuget/v/Microcks.Testcontainers?color=blue&style=for-the-badge)](https://www.nuget.org/packages/Microcks.Testcontainers)
[![License](https://img.shields.io/github/license/microcks/microcks-testcontainers-java?style=for-the-badge&logo=apache)](https://www.apache.org/licenses/LICENSE-2.0)
[![Project Chat](https://img.shields.io/badge/discord-microcks-pink.svg?color=7289da&style=for-the-badge&logo=discord)](https://microcks.io/discord-invite/)
[![Artifact HUB](https://img.shields.io/endpoint?url=https://artifacthub.io/badge/repository/microcks-uber-image&style=for-the-badge)](https://artifacthub.io/packages/search?repo=microcks-uber-image)
[![CNCF Landscape](https://img.shields.io/badge/CNCF%20Landscape-5699C6?style=for-the-badge&logo=cncf)](https://landscape.cncf.io/?item=app-definition-and-development--application-definition-image-build--microcks)

## Build Status

Latest released version is `0.3.2`.

Current development version is `0.3.3`.

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

-   [Documentation](https://microcks.io/documentation/tutorials/getting-started/)
-   [Microcks Community](https://github.com/microcks/community) and community meeting
-   Join us on [Discord](https://microcks.io/discord-invite/), on [GitHub Discussions](https://github.com/orgs/microcks/discussions) or [CNCF Slack #microcks channel](https://cloud-native.slack.com/archives/C05BYHW1TNJ)

To get involved with our community, please make sure you are familiar with the project's [Code of Conduct](./CODE_OF_CONDUCT.md).

## How to use it?

### Include it into your project dependencies

```
dotnet add package Microcks.Testcontainers --version 0.3.2
```

### Startup the container

You just have to specify the container image you'd like to use. This library requires a Microcks `uber` distribution (with no MongoDB dependency).

```csharp
MicrocksContainer container = new MicrocksBuilder()
    .WithImage("quay.io/microcks/microcks-uber:1.13.0")
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

### Verifying mock endpoint has been invoked

Once the mock endpoint has been invoked, you'd probably need to ensure that the mock have been really invoked.

You can do it like this:

```csharp
var invoked = container.Verify("API Pastries", "0.0.1");
```

Or like this:

```csharp
var serviceInvocationsCount = container.GetServiceInvocationsCount("API Pastries", "0.0.1");
```

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

In addition, you can use the `GetMessagesForTestCaseAsync()` method to retrieve the messages exchanged during the test.

**Important:** You must host the API on a port that is accessible from the Microcks container. If your tests are using WebApplicationFactory, the API is hosted on an in-memory server and does not expose a port.

One way to do this is to specify a URL to the WebApplication. However, this requires not to use the minimal hosting model (Program.cs without Main and without Startup.cs).

Refactor your `Program.cs` to use Main and create a new class ApplicationBuilder for example, and copy the content of your `Program.cs` into it.

See below an example of a minimal hosting model with a `Main` method:

```csharp
public class Program
{
    public static void Main(string[] args)
    {
        var app = ApplicationBuilder.Create(args);
        app.Run();
    }
}
```

Then, in your `ApplicationBuilder` class:

```csharp
public static WebApplication Create(params string[] args)
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddControllers();
    // Example: Configure OpenTelemetry, HTTP clients, controllers, Swagger, etc.
    // ...existing code...

    var app = builder.Build();

    app.MapControllers();

    // Example: Enable Swagger in development, HTTPS redirection, authorization, etc.
    // ...existing code...

    return app;
}
```

Finally, in your test class, you can use the `ApplicationBuilder` to create a new instance of your application and specify the URL:

```csharp
using var app = ApplicationBuilder.Create();
app.Urls.Add("http://127.0.0.1:0");
await app.StartAsync();
// Get the port assigned by Kestrel
var port = app.Services.GetRequiredService<IServer>()
    .Features
    .Get<IServerAddressesFeature>()
    .Addresses
    .First()
    .Split(':')
    .Last();

// Expose the port to the Microcks container
IEnumerable<ushort> ports = [(ushort)port];
await TestcontainersSettings.ExposeHostPortsAsync(ports)
    .ConfigureAwait(false);
```

### Advanced features with MicrocksContainersEnsemble

The `MicrocksContainer` referenced above supports essential features of Microcks provided by the main Microcks container.
The list of supported features is the following:

-   Mocking of REST APIs using different kinds of artifacts,
-   Contract-testing of REST APIs using `OPEN_API_SCHEMA` runner/strategy,
-   Mocking and contract-testing of SOAP WebServices,
-   Mocking and contract-testing of GraphQL APIs,
-   Mocking and contract-testing of gRPC APIs.

To support features like Asynchronous API and `POSTMAN`contract-testing, we introduced `MicrocksContainersEnsemble` that allows managing
additional Microcks services. `MicrocksContainersEnsemble` allow you to implement
[Different levels of API contract testing](https://medium.com/@lbroudoux/different-levels-of-api-contract-testing-with-microcks-ccc0847f8c97)
in the Inner Loop with Testcontainers!

A `MicrocksContainersEnsemble` presents the same methods as a `MicrocksContainer`.
You can create and build an ensemble that way:

```csharp
MicrocksContainersEnsemble ensemble = new MicrocksContainerEnsemble(network, MicrocksImage)
    .WithMainArtifacts("apipastries-openapi.yaml")
    .WithSecondaryArtifacts("apipastries-postman-collection.json");

await ensemble.StartAsync();
```

A `MicrocksContainer` is wrapped by an ensemble and is still available to import artifacts and execute test methods.
You have to access it using:

```csharp
MicrocksContainer microcks = ensemble.MicrocksContainer;
microcks.ImportAsMainArtifact(...);
```

To activate async features (WebSocket), you can use `WithAsyncFeature()` method.

```csharp
MicrocksContainersEnsemble ensemble = new MicrocksContainerEnsemble(network, MicrocksImage)
    .WithMainArtifacts("pastry-orders-asyncapi.yml")
    .WithAsyncFeature();

await ensemble.StartAsync();
```

#### Postman contract-testing

On this `ensemble` you may want to enable additional features such as Postman contract-testing:

```csharp
ensemble.WithPostman();
await ensemble.StartAsync();
```

You can execute a `POSTMAN` test using an ensemble that way:

```csharp
var testRequest = new TestRequest
{
    ServiceId = "API Pastries:0.0.1",
    RunnerType = TestRunnerType.POSTMAN,
    TestEndpoint = "http://good-impl:3003",
    Timeout = TimeSpan.FromSeconds(3)
};

TestResult testResult = await _ensemble.MicrocksContainer.TestEndpointAsync(testRequest);
```

#### Asynchronous API support

Asynchronous API feature needs to be explicitly enabled as well.
In case you want to use it for mocking purposes,
you'll have to specify additional connection details to the broker of your choice.

To add a note indicating that it is not necessary to call `WithAsyncFeature()` when an additional method exists.

See an example below with connection to a Kafka broker:

```csharp
KafkaContainer kafkaContainer new KafkaBuilder()
            .WithImage(KafkaImage)
            .WithNetwork(network)
            .WithNetworkAliases("kafka")
            .WithListener("kafka:19092")
            .Build();

await kafkaContainer.StartAsync();

MicrocksContainersEnsemble ensemble = new MicrocksContainerEnsemble(network, MicrocksImage)
    .WithMainArtifacts("pastry-orders-asyncapi.yml")
    .WithKafkaConnection(new KafkaConnection($"kafka:19092"));

await ensemble.StartAsync();
```

As you can see we start the `KafkaContainer` with a specific listener, you should provide the same listener as the `KafkaConnection` this will permit `MicrocksEnsemble` to connect to the `KafkaContainer`.

##### Using mock endpoints for your dependencies

Once started, the `ensemble.AsyncMinionContainer` provides methods for retrieving mock endpoint names for the different
supported protocols (WebSocket, Kafka, etc. ...).

```csharp
string kafkaTopic = ensemble.AsyncMinionContainer
    .GetKafkaMockTopic("Pastry orders API", "0.1.0", "SUBSCRIBE pastry/orders");
```

##### Launching new contract-tests

Using contract-testing techniques on Asynchronous endpoints may require a different style of interacting with the Microcks
container. For example, you may need to:

1. Start the test making Microcks listen to the target async endpoint,
2. Activate your System Under Tests so that it produces an event,
3. Finalize the Microcks tests and actually ensure you received one or many well-formed events.

For that the `MicrocksContainer` now provides a `TestEndpointAsync(TestRequest request)` method that actually returns a `Task<TestResult>`.
Once invoked, you may trigger your application events and then `await` the future result to assert like this:

```csharp
// Start the test, making Microcks listen the endpoint provided in testRequest
Task<TestResult> testResultFuture = ensemble.MicrocksContainer.TestEndpointAsync(testRequest);

// Here below: activate your app to make it produce events on this endpoint.
// myapp.InvokeBusinessMethodThatTriggerEvents();

// Now retrieve the final test result and assert.
TestResult testResult = await testResultFuture;
testResult.IsSuccess.Should().BeTrue();
```

In addition, you can use the `GetEventMessagesForTestCaseAsync()` method to retrieve the events received during the test.
