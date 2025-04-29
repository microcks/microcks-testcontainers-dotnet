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
/// Domain object representing a microservice operation / action invocation response.
/// </summary>
public class Message
{

    /// <summary>
    /// Name of the message.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; }

    /// <summary>
    /// Content of the message.
    /// </summary>
    [JsonPropertyName("content")]
    public string Content { get; set; }

    /// <summary>
    /// Operation ID of the message.
    /// </summary>
    [JsonPropertyName("operationId")]
    public string OperationId { get; set; }

    /// <summary>
    /// Test case ID of the message.
    /// </summary>
    [JsonPropertyName("testCaseId")]
    public string TestCaseId { get; set; }


    /// <summary>
    /// Source Artifact of the message.
    /// </summary>
    [JsonPropertyName("sourceArtifact")]
    public string SourceArtifact { get; set; }

    /// <summary>
    /// Headers of the event message.
    /// </summary>
    [JsonPropertyName("headers")]
    public List<Header> Headers { get; set; }
}