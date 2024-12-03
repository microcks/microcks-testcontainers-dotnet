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

using System.Collections.Generic;
using System.Text;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Networks;
using Testcontainers.Kafka;

namespace Microcks.Testcontainers.Tests.Async.Kafka;

public static class KafkaContainerHelper
{
    // TODO: Simplify this code after PR is merged https://github.com/testcontainers/testcontainers-dotnet/pull/1316
    public static KafkaContainer CreateKafkaContainer(INetwork network)
    {
        return new KafkaBuilder()
            .WithImage("confluentinc/cp-kafka:7.8.0")
            .WithNetwork(network)
            .WithNetworkAliases("kafka")
            .WithEnvironment(new Dictionary<string, string>
            {
                {
                    "KAFKA_LISTENERS",
                    $"PLAINTEXT://0.0.0.0:{KafkaBuilder.KafkaPort},BROKER://0.0.0.0:{KafkaBuilder.BrokerPort},CONTROLLER://0.0.0.0:9094,TC-0://kafka:19092"
                },
                {
                    "KAFKA_LISTENER_SECURITY_PROTOCOL_MAP",
                    "PLAINTEXT:PLAINTEXT,BROKER:PLAINTEXT,CONTROLLER:PLAINTEXT,TC-0:PLAINTEXT"
                }
            })
            .WithStartupCallback((container, ct) =>
            {
                const string advertisedListener = ",TC-0://kafka:19092";

                const char lf = '\n';
                var startupScript = new StringBuilder();

                startupScript.Append("#!/bin/bash");
                startupScript.Append(lf);
                startupScript.Append("echo 'clientPort=" + KafkaBuilder.ZookeeperPort + "' > zookeeper.properties");
                startupScript.Append(lf);
                startupScript.Append("echo 'dataDir=/var/lib/zookeeper/data' >> zookeeper.properties");
                startupScript.Append(lf);
                startupScript.Append("echo 'dataLogDir=/var/lib/zookeeper/log' >> zookeeper.properties");
                startupScript.Append(lf);
                startupScript.Append("zookeeper-server-start zookeeper.properties &");
                startupScript.Append(lf);
                startupScript.Append("export KAFKA_ADVERTISED_LISTENERS=PLAINTEXT://" + container.Hostname + ":" + container.GetMappedPublicPort(KafkaBuilder.KafkaPort) + ",BROKER://" + container.IpAddress + ":" + KafkaBuilder.BrokerPort + advertisedListener);
                startupScript.Append(lf);
                startupScript.Append("echo '' > /etc/confluent/docker/ensure");
                startupScript.Append(lf);
                startupScript.Append("exec /etc/confluent/docker/run");
                return container.CopyAsync(Encoding.Default.GetBytes(startupScript.ToString()),
                    KafkaBuilder.StartupScriptFilePath, Unix.FileMode755, ct);
            })
            .Build();
    }

}
