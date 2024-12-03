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
/// Represents a connection to a Kafka broker.
/// </summary>
public class KafkaConnection
{
    /// <summary>
    /// Initializes a new instance of the <see cref="KafkaConnection"/> class with the specified bootstrap servers.
    /// </summary>
    /// <param name="bootstrapServers">The Kafka bootstrap servers.</param>
    public KafkaConnection(string bootstrapServers)
    {
        this.BootstrapServers = bootstrapServers;
    }

    /// <summary>
    /// Gets the Kafka bootstrap servers.
    /// </summary>
    public string BootstrapServers { get; }
}
