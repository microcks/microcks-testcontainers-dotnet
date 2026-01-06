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

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[assembly: InternalsVisibleTo("Microcks.Testcontainers.Tests")]

namespace Microcks.Testcontainers.Helpers;

/// <summary>
/// A temporary helper class for MacOS to fix SIGILL error.
/// Note: This is a temporary fix and should be removed in the near future because Java 21.0.7 has not yet landed into the Red Hat base image we're using for Microcks but it should come very soon.
/// </summary>
public static class MacOSHelper
{
    /// <summary>
    /// Gets a value indicating whether the current process is running on macOS (Darwin) on Arm64.
    /// </summary>
    public static bool IsMacOS { get; internal set; }
        = RuntimeInformation.ProcessArchitecture == Architecture.Arm64
          && RuntimeInformation.OSDescription.Contains("Darwin");

    /// <summary>
    /// Get Java options for MacOS M4 to fix SIGILL error.
    /// </summary>
    /// <returns></returns>
    public static IReadOnlyDictionary<string, string> GetJavaOptions(string keyname = "JAVA_OPTIONS")
    {
        if (IsMacOS)
        {
            return new Dictionary<string, string>
            {
                { keyname, "-XX:UseSVE=0" }
            };
        }

        return new Dictionary<string, string>();
    }
}
