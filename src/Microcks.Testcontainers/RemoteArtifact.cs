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

/// <summary>
/// Represents a remote artifact with its URL and associated secret name.
/// </summary>
public class RemoteArtifact
{
    /// <summary>
    /// URL of remote artifact.
    /// </summary>
    public string Url { get; set; }

    /// <summary>
    /// Secret name to access remote artifact.
    /// </summary>
    public string SecretName { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="RemoteArtifact" /> class.
    /// </summary>
    /// <param name="url">URL of remote artifact.</param>
    /// <param name="secretName">Secret name to access remote artifact.</param>
    public RemoteArtifact(string url, string secretName)
    {
        Url = url;
        SecretName = secretName;
    }
}