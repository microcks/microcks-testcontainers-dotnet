using Microcks.Testcontainers.Converter;
using System.Text.Json.Serialization;

namespace Microcks.Testcontainers.Model;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
/// <summary>
/// TestResult model for Microcks.
/// </summary>
public class TestResult
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("version")]
    public int Version { get; set; }

    [JsonPropertyName("testNumber")]
    public int TestNumber { get; set; }

    [JsonPropertyName("testDate")]
    public long TestDate { get; set; }

    [JsonPropertyName("testedEndpoint")]
    public string TestedEndpoint { get; set; }

    [JsonPropertyName("serviceId")]
    public string ServiceId { get; set; }

    [JsonConverter(typeof(TimeSpanToMillisecondsConverter))]
    [JsonPropertyName("timeout")]
    public TimeSpan Timeout { get; set; }

    [JsonPropertyName("elapsedTime")]
    public int ElapsedTime { get; set; }

    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("inProgress")]
    public bool InProgress { get; set; }

    [JsonPropertyName("runnerType")]
    public TestRunnerType RunnerType { get; set; }

    [JsonPropertyName("testCaseResults")]
    public List<TestCaseResult> TestCaseResults { get; set; }

    [JsonPropertyName("operationsHeaders")]
    public Dictionary<string, List<Header>> OperationsHeaders { get; set; }
}
