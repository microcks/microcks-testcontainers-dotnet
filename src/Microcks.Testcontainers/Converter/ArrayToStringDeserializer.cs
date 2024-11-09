using System.Text.Json;
using System.Text.Json.Serialization;

namespace Microcks.Testcontainers.Converter;

/// <summary>
/// This class is used to convert an array of strings to a single string.
/// </summary>
public class ArrayToStringConverter : JsonConverter<string>
{
    /// <inheritdoc />
    public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.StartArray)
        {
            var values = new List<string>();
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndArray)
                {
                    break;
                }
                values.Add(reader.GetString());
            }
            return string.Join(",", values);
        }
        return reader.GetString();
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value);
    }
}
