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


namespace Microcks.Testcontainers;

/// <inheritdoc cref="DockerContainer" />
public sealed class MicrocksAsyncMinionContainer : DockerContainer
{
    private const string DESTINATION_PATTERN = "{0}-{1}-{2}";

    public MicrocksAsyncMinionContainer(MicrocksAsyncMinionConfiguration configuration)
        : base(configuration)
    {
    }


    public string GetKafkaMockTopic(string service, string version, string operationName)
    {
        // operationName may start with SUBSCRIBE or PUBLISH.
        if (operationName.Contains(" "))
        {
            operationName = operationName.Split(' ')[1];
        }
        return String.Format(DESTINATION_PATTERN,
            service.Replace(" ", "").Replace("-", ""),
            version,
            operationName.Replace("/", "-"));
    }
}
