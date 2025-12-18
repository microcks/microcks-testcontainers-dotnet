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

namespace Microcks.Testcontainers.Helpers;

/// <summary>
/// Configuration constants for Microcks Testcontainers.
/// </summary>
public class ConfigurationConstants
{
    /// <summary>
    /// Environment variable name used to configure Microcks logging level for the Microcks package.
    /// </summary>
    public const string MicrocksLoggingLevelEnvVar = "LOGGING_LEVEL_IO_GITHUB_MICROCKS";

    /// <summary>
    /// Environment variable value for enabling DEBUG logging.
    /// </summary>
    public const string DebugLogLevelEnvVar = "DEBUG";

    /// <summary>
    /// Environment variable name used by Quarkus to configure console log level.
    /// </summary>
    public const string QuarkusConsoleLogLevelEnvVar = "QUARKUS_LOG_CONSOLE_LEVEL";

    /// <summary>
    /// Environment variable name used by Quarkus to configure the Microcks category log level.
    /// </summary>
    public const string QuarkusMicrocksCategoryLogLevelEnvVar = "QUARKUS_LOG_CATEGORY__IO_GITHUB_MICROCKS__LEVEL";
}
