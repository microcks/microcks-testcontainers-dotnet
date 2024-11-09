using System.Threading.Tasks;
namespace Microcks.Testcontainers;

/// <inheritdoc cref="DockerContainer" />
public sealed class MicrocksContainer : DockerContainer
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MicrocksContainer" /> class.
    /// </summary>
    /// <param name="configuration">The container configuration.</param>
    public MicrocksContainer(MicrocksConfiguration configuration)
        : base(configuration)
    {
        Starting += (_, _) => Logger.LogInformation("MicrocksContainer container is starting, performing configuration.");
        Started += (_, _) => Logger.LogInformation("MicrocksContainer container is ready! UI available at {Url}.", GetHttpEndpoint());
    }

    /// <summary>
    /// Returns the GraphQL Base URL for the Microcks container.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="version"></param>
    /// <returns></returns>
    public Uri GetGraphQLMockEndpoint(string name, string version)
    {
        return new UriBuilder(GetHttpEndpoint())
        {
            Path = $"graphql/{name}/{version}"
        }.Uri;
    }

    /// <summary>
    /// Returns the gRPC Base URL for the Microcks container.
    /// </summary>
    /// <returns></returns>
    public Uri GetGrpcMockEndpoint()
    {
        return new UriBuilder("grpc", Hostname, GetMappedPublicPort(MicrocksBuilder.MicrocksGrpcPort)).Uri;
    }

    /// <summary>
    /// Obtains the HTTP endpoint for the Microcks container.
    /// </summary>
    /// <returns>HTTP endpoint address</returns>
    public Uri GetHttpEndpoint()
    {
        return new UriBuilder(
            Uri.UriSchemeHttp,
            Hostname,
            GetMappedPublicPort(MicrocksBuilder.MicrocksHttpPort)
        ).Uri;
    }

    /// <summary>
    /// Returns the REST Base URL for the Microcks container.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="version"></param>
    /// <returns></returns>
    public Uri GetRestMockEndpoint(string name, string version)
    {
        return new UriBuilder(GetHttpEndpoint())
        {
            Path = $"rest/{name}/{version}"
        }.Uri;
    }

    /// <summary>
    /// Returns the SOAP Base URL for the Microcks container.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="version"></param>
    /// <returns></returns>
    public Uri GetSoapMockEndpoint(string name, string version)
    {
        return new UriBuilder(GetHttpEndpoint())
        {
            Path = $"soap/{name}/{version}"
        }.Uri;
    }

    /// <summary>
    /// Import a main artifact into the Microcks container after it has started.
    /// </summary>
    /// <param name="artifact"></param>
    public void ImportAsMainArtifact(string artifact)
    {
        this.ImportArtifactAsync(artifact, true).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Import a secondary artifact into the Microcks container after it has started.
    /// </summary>
    /// <param name="artifact"></param>
    public void ImportAsSecondaryArtifact(string artifact)
    {
        this.ImportArtifactAsync(artifact, false).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Dispose of the Microcks container.
    /// </summary>
    /// <returns></returns>
    protected override ValueTask DisposeAsyncCore()
    {
        return base.DisposeAsyncCore();
    }
}
