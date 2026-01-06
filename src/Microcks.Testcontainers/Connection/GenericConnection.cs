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

namespace Microcks.Testcontainers.Connection;

/// <summary>
/// Represents generic connection information (URL and optional credentials).
/// </summary>
public class GenericConnection
{
    /// <summary>
    /// Gets the connection URL.
    /// </summary>
    public string Url { get; }

    /// <summary>
    /// Gets the username used for authentication, if any.
    /// </summary>
    public string Username { get; }

    /// <summary>
    /// Gets the password used for authentication, if any.
    /// </summary>
    public string Password { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="GenericConnection"/> class.
    /// </summary>
    /// <param name="url">The connection URL.</param>
    /// <param name="username">The username used for authentication, if any.</param>
    /// <param name="password">The password used for authentication, if any.</param>
    public GenericConnection(string url, string username = null, string password = null)
    {
        Url = url;
        Username = username;
        Password = password;
    }
}
