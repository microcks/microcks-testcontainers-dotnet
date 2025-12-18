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

using Microcks.Testcontainers.Helpers;

namespace Microcks.Testcontainers.Tests.DebugLogLevel;

/// <summary>
/// Tests for the WithDebugLogLevel feature on MicrocksContainerEnsemble.
/// Verifies that debug logging environment variables are correctly set or not set
/// on both Microcks (Spring) and Async Minion (Quarkus) containers.
/// </summary>
[Collection("DisableParallelization")]
public sealed class MicrocksDebugLogLevelTests
{
    /// <summary>
    /// Verifies that when WithDebugLogLevel() is called, the appropriate debug
    /// environment variables are set on both Microcks and Async Minion containers.
    /// - Microcks (Spring): LOGGING_LEVEL_IO_GITHUB_MICROCKS=DEBUG
    /// - Async Minion (Quarkus): QUARKUS_LOG_CONSOLE_LEVEL=DEBUG and 
    ///   QUARKUS_LOG_CATEGORY__IO_GITHUB_MICROCKS__LEVEL=DEBUG
    /// </summary>
    [Fact]
    public async Task WithDebugLogLevel_ShouldSetEnvironmentVariables()
    {
        // Arrange - Create ensemble WITH WithDebugLogLevel()
        await using var ensemble = new MicrocksContainerEnsemble(MicrocksBuilder.MicrocksImage)
            .WithDebugLogLevel()
            .WithAsyncFeature();

        await ensemble.StartAsync(TestContext.Current.CancellationToken);

        // Assert - Use Docker Inspect API to verify environment variables
        using var dockerClient = new Docker.DotNet.DockerClientConfiguration().CreateClient();

        // Verify Microcks container has the debug environment variable
        var microcksInspect = await dockerClient.Containers.InspectContainerAsync(
            ensemble.MicrocksContainer.Id,
            TestContext.Current.CancellationToken);

        var expectedMicrocksEnvVar = $"{ConfigurationConstants.MicrocksLoggingLevelEnvVar}={ConfigurationConstants.DebugLogLevelEnvVar}";
        Assert.Contains(expectedMicrocksEnvVar, microcksInspect.Config.Env);

        // Verify Async Minion container has the Quarkus debug environment variables
        var minionInspect = await dockerClient.Containers.InspectContainerAsync(
            ensemble.AsyncMinionContainer.Id,
            TestContext.Current.CancellationToken);

        var expectedConsoleLogLevel = $"{ConfigurationConstants.QuarkusConsoleLogLevelEnvVar}={ConfigurationConstants.DebugLogLevelEnvVar}";
        var expectedMicrocksLogLevel = $"{ConfigurationConstants.QuarkusMicrocksCategoryLogLevelEnvVar}={ConfigurationConstants.DebugLogLevelEnvVar}";

        Assert.Contains(expectedConsoleLogLevel, minionInspect.Config.Env);
        Assert.Contains(expectedMicrocksLogLevel, minionInspect.Config.Env);
    }

    /// <summary>
    /// Verifies that when WithDebugLogLevel() is NOT called, the debug environment
    /// variables are not present on the containers. This ensures the default behavior
    /// does not enable debug logging.
    /// </summary>
    [Fact]
    public async Task WithoutDebugLogLevel_ShouldNotSetEnvironmentVariables()
    {
        // Arrange - Create ensemble WITHOUT WithDebugLogLevel()
        await using var ensemble = new MicrocksContainerEnsemble(MicrocksBuilder.MicrocksImage)
            .WithAsyncFeature();

        await ensemble.StartAsync(TestContext.Current.CancellationToken);

        // Assert - Use Docker Inspect API to verify environment variables are NOT set
        using var dockerClient = new Docker.DotNet.DockerClientConfiguration().CreateClient();

        // Verify Microcks container does NOT have the debug environment variable
        var microcksInspect = await dockerClient.Containers.InspectContainerAsync(
            ensemble.MicrocksContainer.Id,
            TestContext.Current.CancellationToken);

        Assert.DoesNotContain(microcksInspect.Config.Env,
            env => env.StartsWith($"{ConfigurationConstants.MicrocksLoggingLevelEnvVar}="));

        // Verify Async Minion container does NOT have the Quarkus debug environment variables
        var minionInspect = await dockerClient.Containers.InspectContainerAsync(
            ensemble.AsyncMinionContainer.Id,
            TestContext.Current.CancellationToken);

        Assert.DoesNotContain(minionInspect.Config.Env,
            env => env.StartsWith($"{ConfigurationConstants.QuarkusConsoleLogLevelEnvVar}="));
        Assert.DoesNotContain(minionInspect.Config.Env,
            env => env.StartsWith($"{ConfigurationConstants.QuarkusMicrocksCategoryLogLevelEnvVar}="));
    }
}
