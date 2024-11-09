using Microcks.Testcontainers.Converter;
using System.Text.Json.Serialization;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Microcks.Testcontainers.Model;

/// <summary>
/// Microcks Contrat for a header.
/// </summary>
public class Header
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("values")]
    [JsonConverter(typeof(ArrayToStringConverter))]
    public string Values { get; set; }

}
