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
/// Represents an event message with details such as name, content, test case ID, headers, ID, and media type.
/// </summary>
public class EventMessage
{
    /// <summary>
    /// Name of the event message.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; }

    /// <summary>
    /// Content of the event message.
    /// </summary>
    [JsonPropertyName("content")]
    public string Content { get; set; }

    /// <summary>
    /// Test case ID of the event message.
    /// </summary>
    [JsonPropertyName("testCaseId")]
    public string TestCaseId { get; set; }

    /// <summary>
    /// Headers of the event message.
    /// </summary>
    [JsonPropertyName("headers")]
    public List<string> Headers { get; set; }

    /// <summary>
    /// ID of the event message.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; }

    /// <summary>
    /// Media type of the event message.
    /// </summary>
    [JsonPropertyName("mediaType")]
    public string MediaType { get; set; }
}
