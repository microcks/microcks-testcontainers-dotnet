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

using System.Text.Json.Serialization;

namespace Microcks.Testcontainers.Model;

/// <summary>
/// Represents a unidirectional event with a type and an associated event message.
/// </summary>
public class UnidirectionalEvent
{
    /// <summary>
    /// Gets or sets the type of the unidirectional event.
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; }

    /// <summary>
    /// Gets or sets the event message associated with the unidirectional event.
    /// </summary>
    [JsonPropertyName("eventMessage")]
    public EventMessage EventMessage { get; set; }
}
