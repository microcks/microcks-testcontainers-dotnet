using Microcks.Testcontainers.Converter;
using System.Text.Json.Serialization;

namespace Microcks.Testcontainers.Model;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
/// <summary>
/// TestRequest Model for Microcks.
/// </summary>
public class TestRequest
{
    [JsonPropertyName("serviceId")]
    public string ServiceId { get; set; }

    [JsonPropertyName("runnerType")]
    public TestRunnerType RunnerType { get; set; }

    [JsonPropertyName("testEndpoint")]
    public string TestEndpoint { get; set; }

    [JsonPropertyName("timeout")]
    [JsonConverter(typeof(TimeSpanToMillisecondsConverter))]
    public TimeSpan Timeout { get; set; }

    [JsonPropertyName("filteredOperations")]
    public List<string> FilteredOperations { get; set; }

    [JsonPropertyName("operationsHeaders")]
    public Dictionary<string, List<Header>> OperationsHeaders { get; set; }

    [JsonPropertyName("oAuth2Context")]
    public OAuth2ClientContext oAuth2Context { get; set; }
}
