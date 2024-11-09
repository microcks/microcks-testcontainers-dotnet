using System.Text.Json;
using System.Text.Json.Serialization;

namespace Microcks.Testcontainers.Converter;

/// <summary>
/// This class implements a custom JSON converter for <see cref="TimeSpan"/> to milliseconds.
/// </summary>
public class TimeSpanToMillisecondsConverter : JsonConverter<TimeSpan>
{
    /// <inheritdoc />
    public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return TimeSpan.FromMilliseconds(reader.GetInt64());
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value.TotalMilliseconds);
    }
}
