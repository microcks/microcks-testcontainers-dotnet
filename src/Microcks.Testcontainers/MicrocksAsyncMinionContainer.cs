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

using System.Diagnostics.CodeAnalysis;

namespace Microcks.Testcontainers;

/// <inheritdoc cref="DockerContainer" />
public sealed class MicrocksAsyncMinionContainer : DockerContainer
{
    private const string DestinationPattern = "{0}-{1}-{2}";

    /// <summary>
    /// Initializes a new instance of the <see cref="MicrocksAsyncMinionContainer" /> class.
    /// </summary>
    /// <param name="configuration">The container configuration.</param>
    public MicrocksAsyncMinionContainer(MicrocksAsyncMinionConfiguration configuration)
        : base(configuration)
    {
    }

    /// <summary>
    /// Generates a Kafka mock topic name based on the provided service, version, and operation name.
    /// </summary>
    /// <param name="service">The name of the service.</param>
    /// <param name="version">The version of the service.</param>
    /// <param name="operationName">The name of the operation, which may start with SUBSCRIBE or PUBLISH.</param>
    /// <returns>A formatted Kafka mock topic name.</returns>
    [SuppressMessage("Style", "S2325", Justification = "Kept as instance method for backward compatibility")]
    public string GetKafkaMockTopic(string service, string version, string operationName)
    {
        operationName = ExtractOperationName(operationName);

        return String.Format(DestinationPattern,
            service.Replace(" ", "").Replace("-", ""),
            version,
            operationName.Replace("/", "-"));
    }

    /// <summary>
    /// Returns the WebSocket mock endpoint based on the provided service, version, and operation name.
    /// </summary>
    /// <returns>The WebSocket mock endpoint.</returns>
    public Uri GetWebSocketMockEndpoint(string service, string version, string operationName)
    {
        operationName = ExtractOperationName(operationName);
        var port = this.GetMappedPublicPort(MicrocksAsyncMinionBuilder.MicrocksAsyncMinionHttpPort);
        var escapedService = service.Replace(" ", "+");
        var escapedVersion = version.Replace(" ", "+");

        return new Uri($"ws://{this.Hostname}:{port}/api/ws/{escapedService}/{escapedVersion}/{operationName}");
    }

    /// <summary>
    /// Extracts the operation name from the provided operation name.
    /// </summary>
    /// <param name="operationName">operationName may start with SUBSCRIBE or PUBLISH.</param>
    /// <returns>The extracted operation name.</returns>
    private static string ExtractOperationName(string operationName)
    {
        if (operationName.Contains(' '))
        {
            operationName = operationName.Split(' ')[1];
        }
        return operationName;
    }
}
