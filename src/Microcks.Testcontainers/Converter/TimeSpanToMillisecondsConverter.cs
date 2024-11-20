//
// Copyright The Microcks Authors.
//
// Licensed under the Apache License, Version 2.0 (the "License")
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//  http://www.apache.org/licenses/LICENSE-2.0 
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
//

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
