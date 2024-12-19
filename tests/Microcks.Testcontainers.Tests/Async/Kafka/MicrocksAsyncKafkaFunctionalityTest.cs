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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Confluent.Kafka;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Networks;
using FluentAssertions;
using Microcks.Testcontainers.Connection;
using Microcks.Testcontainers.Model;
using Testcontainers.Kafka;

namespace Microcks.Testcontainers.Tests.Async.Kafka;

public sealed class MicrocksAsyncKafkaFunctionalityTest : IAsyncLifetime
{
    /// <summary>
    /// Image name for the Microcks container.
    /// </summary>
    private const string MicrocksImage = "quay.io/microcks/microcks-uber:1.10.0";

    private MicrocksContainerEnsemble _microcksContainerEnsemble;

    private KafkaContainer _kafkaContainer;

    public async Task DisposeAsync()
    {
        await this._microcksContainerEnsemble.DisposeAsync();
        await this._kafkaContainer.DisposeAsync();
    }

    public async Task InitializeAsync()
    {
        var network = new NetworkBuilder().Build();

        this._kafkaContainer = CreateKafkaContainer(network);

        // Start the Kafka container
        await this._kafkaContainer.StartAsync().ConfigureAwait(false);

        this._microcksContainerEnsemble = new MicrocksContainerEnsemble(network, MicrocksImage)
            .WithMainArtifacts("pastry-orders-asyncapi.yml")
            .WithKafkaConnection(new KafkaConnection($"kafka:19092"));

        await this._microcksContainerEnsemble.StartAsync();
    }

    // TODO: Simplify this code after PR is merged https://github.com/testcontainers/testcontainers-dotnet/pull/1316
    private KafkaContainer CreateKafkaContainer(INetwork network)
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

    [Fact]
    public void ShouldReceivedKafkaMessageWhenMessageIsEmitted()
    {
        const string expectedMessage = "{\"id\":\"4dab240d-7847-4e25-8ef3-1530687650c8\",\"customerId\":\"fe1088b3-9f30-4dc1-a93d-7b74f0a072b9\",\"status\":\"VALIDATED\",\"productQuantities\":[{\"quantity\":2,\"pastryName\":\"Croissant\"},{\"quantity\":1,\"pastryName\":\"Millefeuille\"}]}";
        var kafkaTopic = this._microcksContainerEnsemble.AsyncMinionContainer
            .GetKafkaMockTopic("Pastry orders API", "0.1.0", "SUBSCRIBE pastry/orders");

        var bootstrapServers = this._kafkaContainer.GetBootstrapAddress()
            .Replace("PLAINTEXT://", "", StringComparison.OrdinalIgnoreCase);

        // Initialize Kafka consumer to receive message
        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = bootstrapServers,
            GroupId = $"test-group-{DateTime.Now.Ticks}",
            ClientId = $"test-client-{DateTime.Now.Ticks}",

            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false,
        };

        using var consumer = new ConsumerBuilder<string, string>(consumerConfig)
            .SetKeyDeserializer(Deserializers.Utf8)
            .SetValueDeserializer(Deserializers.Utf8)
            .SetErrorHandler((_, e) =>
            {
                Debug.WriteLine($"Error: {e.Reason}");
            })
            .Build();

        consumer.Subscribe(kafkaTopic);
        string message = null;
        // Consume message from Kafka 4000 milliseconds attempt
        var consumeResult = consumer.Consume(TimeSpan.FromMilliseconds(5000));

        if (consumeResult != null)
        {
            message = consumeResult.Message.Value;
        }

        message.Should().Be(expectedMessage);
    }

    [Theory]
    [MemberData(nameof(ContractData))]
    public async Task ShouldReturnsCorrectStatusContractWhenMessageIsEmitted(
        string message,
        bool result,
        string? expectedMessage)
    {
        var testRequest = new TestRequest
        {
            ServiceId = "Pastry orders API:0.1.0",
            RunnerType = TestRunnerType.ASYNC_API_SCHEMA,
            TestEndpoint = "kafka://kafka:19092/pastry-orders",
            Timeout = TimeSpan.FromMilliseconds(20001)
        };

        // Init Kafka producer to send a message
        var producerConfig = new ProducerConfig
        {
            BootstrapServers = this._kafkaContainer.GetBootstrapAddress()
                .Replace("PLAINTEXT://", "", StringComparison.OrdinalIgnoreCase),
            ClientId = $"test-client-{DateTime.Now.Ticks}",
        };

        using var producer = new ProducerBuilder<string, string>(producerConfig)
            .SetKeySerializer(Serializers.Utf8)
            .SetValueSerializer(Serializers.Utf8)
            .SetErrorHandler((_, e) =>
            {
                Debug.WriteLine($"Error: {e.Reason}");
            })
            .SetLogHandler((_, logMessage) =>
            {
                Debug.WriteLine($"{logMessage.Name} sending {logMessage.Message}");
            })
            .Build();

        var taskTestResult = Task.Run(() => this._microcksContainerEnsemble
            .MicrocksContainer
            .TestEndpointAsync(testRequest));

        // Act
        for (var i = 0; i < 5; i++)
        {
            producer.Produce("pastry-orders", new Message<string, string>
            {
                Key = Guid.NewGuid().ToString(),
                Value = message
            });
            // producer.Flush();
            await Task.Delay(500);
        }

        // Wait for a test result
        var testResult = await taskTestResult;

        // Assert
        testResult.Success.Should().Be(result);
        testResult.TestedEndpoint.Should().Be(testRequest.TestEndpoint);

        var testCaseResult = testResult.TestCaseResults.First();
        var testStepResults = testCaseResult.TestStepResults;

        testStepResults.Should().NotBeEmpty();

        if( expectedMessage == null )
        {
            testStepResults.First().Message.Should().BeNull();
        }
        else
        {
            testStepResults.First().Message.Should().Contain(expectedMessage);
        }
    }

    public static IEnumerable<object[]> ContractData()
    {
        // Contract data
        // good message
        yield return
        [
            "{\"id\":\"abcd\",\"customerId\":\"efgh\",\"status\":\"CREATED\",\"productQuantities\":[{\"quantity\":2,\"pastryName\":\"Croissant\"},{\"quantity\":1,\"pastryName\":\"Millefeuille\"}]}",
            true,
            null
        ];
        // bad message has no status
        yield return
        [
            "{\"id\":\"abcd\",\"customerId\":\"efgh\",\"productQuantities\":[{\"quantity\":2,\"pastryName\":\"Croissant\"},{\"quantity\":1,\"pastryName\":\"Millefeuille\"}]}",
            false,
            "object has missing required properties ([\"status\"]"
        ];
    }
}
