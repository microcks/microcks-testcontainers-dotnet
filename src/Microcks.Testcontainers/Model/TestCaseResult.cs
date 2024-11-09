using System.Text.Json.Serialization;

namespace Microcks.Testcontainers.Model;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
/// <summary>
/// TestCaseResult.
/// </summary>
public class TestCaseResult
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("elapsedTime")]
    public int ElapsedTime { get; set; }

    [JsonPropertyName("operationName")]
    public string OperationName { get; set; }

    [JsonPropertyName("testStepResults")]
    public List<TestStepResult> TestStepResults { get; set; }
}
