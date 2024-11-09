using System.Text.Json.Serialization;

namespace Microcks.Testcontainers.Model;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

/// <summary>
/// TestRunnerType for Microcks.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TestRunnerType
{
    HTTP,
    SOAP_HTTP,
    SOAP_UI,
    POSTMAN,
    OPEN_API_SCHEMA,
    ASYNC_API_SCHEMA,
    GRPC_PROTOBUF,
    GRAPHQL_SCHEMA
}
