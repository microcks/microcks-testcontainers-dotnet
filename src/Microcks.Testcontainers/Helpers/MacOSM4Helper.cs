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

/// <summary>
/// A fatal error has been detected by the Java Runtime Environment:
///
/// SIGILL (0x4) at pc=0x0000ffff6f33fc5c, pid=1, tid=85
/// </summary>
public static class MacOSM4Helper
{
    public static bool IsMacOSM4 { get; internal set; }
        = RuntimeInformation.ProcessArchitecture == Architecture.Arm64
          && RuntimeInformation.OSDescription.Contains("Darwin");

    /// <summary>
    /// Get Java options for MacOS M4 to fix SIGILL error.
    /// </summary>
    /// <returns></returns>
    public static IReadOnlyDictionary<string, string> GetJavaOptions(string keyname = "JAVA_OPTIONS")
    {
        if (IsMacOSM4)
        {
            return new Dictionary<string, string>
            {
                { keyname, "-XX:UseSVE=0" }
            };
        }

        return new Dictionary<string, string>();
    }
}
