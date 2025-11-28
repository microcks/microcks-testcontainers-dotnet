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
using System.Diagnostics;
using System.Linq;
using Confluent.Kafka;
using DotNet.Testcontainers.Builders;
using Microcks.Testcontainers.Connection;
using Microcks.Testcontainers.Model;
using Microsoft.Extensions.Logging;
using Testcontainers.Kafka;
using DotNet.Testcontainers.Networks;

namespace Microcks.Testcontainers.Tests.Async.Kafka;

[Collection(nameof(KafkaCollection))]
public sealed class MicrocksAsyncKafkaFunctionalityTest
    : IClassFixture<MicrocksAsyncKafkaFunctionalityTest.KafkaFixture>
{
    /// <summary>
    /// Image name for the Microcks container.
    /// </summary>
    private const string MicrocksImage = "quay.io/microcks/microcks-uber:1.10.0";

    private readonly KafkaFixture _fixture;

    public MicrocksAsyncKafkaFunctionalityTest(KafkaFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void ShouldReceivedKafkaMessageWhenMessageIsEmitted()
    {
        const string expectedMessage = "{\"id\":\"4dab240d-7847-4e25-8ef3-1530687650c8\",\"customerId\":\"fe1088b3-9f30-4dc1-a93d-7b74f0a072b9\",\"status\":\"VALIDATED\",\"productQuantities\":[{\"quantity\":2,\"pastryName\":\"Croissant\"},{\"quantity\":1,\"pastryName\":\"Millefeuille\"}]}";
        var kafkaTopic = _fixture.MicrocksContainerEnsemble.AsyncMinionContainer
            .GetKafkaMockTopic("Pastry orders API", "0.1.0", "SUBSCRIBE pastry/orders");

        var bootstrapServers = _fixture.KafkaContainer.GetBootstrapAddress()
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
        // Consume message from Kafka 5000 milliseconds attempt
        var consumeResult = consumer.Consume(TimeSpan.FromMilliseconds(5000));

        if (consumeResult != null)
        {
            message = consumeResult.Message.Value;
        }

        Assert.Equal(expectedMessage, message);
    }


    [Fact]
    public async Task ShouldReturnsCorrectStatusContractWhenGoodMessageIsEmitted()
    {
        const string message =
            "{\"id\":\"abcd\",\"customerId\":\"efgh\",\"status\":\"CREATED\",\"productQuantities\":[{\"quantity\":2,\"pastryName\":\"Croissant\"},{\"quantity\":1,\"pastryName\":\"Millefeuille\"}]}";

        var testRequest = new TestRequest
        {
            ServiceId = "Pastry orders API:0.1.0",
            RunnerType = TestRunnerType.ASYNC_API_SCHEMA,
            TestEndpoint = "kafka://kafka:19092/pastry-orders",
            Timeout = TimeSpan.FromSeconds(3)
        };

        // Init Kafka producer to send a message
        var producerConfig = new ProducerConfig
        {
            BootstrapServers = _fixture.KafkaContainer.GetBootstrapAddress()
                .Replace("PLAINTEXT://", "", StringComparison.OrdinalIgnoreCase),
            ClientId = $"test-client-{DateTime.Now.Ticks}",
            Acks = Acks.All
        };

        using var producer = new ProducerBuilder<string, string>(producerConfig)
            .SetKeySerializer(Serializers.Utf8)
            .SetValueSerializer(Serializers.Utf8)
            .SetErrorHandler((_, e) =>
            {
                _fixture.KafkaContainer.Logger.LogError("Error: {Reason}", e.Reason);
            })
            .SetLogHandler((_, logMessage) =>
            {
                _fixture.KafkaContainer.Logger.LogInformation("{Name} sending {Message}", logMessage.Name, logMessage.Message);
            })
            .Build();

        var taskTestResult = _fixture.MicrocksContainerEnsemble
            .MicrocksContainer
            .TestEndpointAsync(testRequest, TestContext.Current.CancellationToken);

        await Task.Delay(750, TestContext.Current.CancellationToken);

        // Act
        for (var i = 0; i < 5; i++)
        {
            producer.Produce("pastry-orders", new Message<string, string>
            {
                Key = Guid.NewGuid().ToString(),
                Value = message
            });
            producer.Flush(TestContext.Current.CancellationToken);
            await Task.Delay(500, TestContext.Current.CancellationToken);
        }

        // Wait for a test result
        var testResult = await taskTestResult;

        // Assert
        Assert.False(testResult.InProgress);
        Assert.True(testResult.Success);
        Assert.Equal(testRequest.TestEndpoint, testResult.TestedEndpoint);

        var testCaseResult = testResult.TestCaseResults.First();
        var testStepResults = testCaseResult.TestStepResults;

        // Minimum 1 message captured
        Assert.NotEmpty(testStepResults);
        // No error message
        Assert.Null(testStepResults.First().Message);
    }


    [Fact]
    public async Task ShouldReturnsCorrectStatusContractWhenBadMessageIsEmitted()
    {
        const string message = "{\"id\":\"abcd\",\"customerId\":\"efgh\",\"productQuantities\":[{\"quantity\":2,\"pastryName\":\"Croissant\"},{\"quantity\":1,\"pastryName\":\"Millefeuille\"}]}";

        var testRequest = new TestRequest
        {
            ServiceId = "Pastry orders API:0.1.0",
            RunnerType = TestRunnerType.ASYNC_API_SCHEMA,
            TestEndpoint = "kafka://kafka:19092/pastry-orders",
            Timeout = TimeSpan.FromMilliseconds(40001)
        };

        // Init Kafka producer to send a message
        var producerConfig = new ProducerConfig
        {
            BootstrapServers = _fixture.KafkaContainer.GetBootstrapAddress()
                .Replace("PLAINTEXT://", "", StringComparison.OrdinalIgnoreCase),
            ClientId = $"test-client-{DateTime.Now.Ticks}",
            Acks = Acks.All
        };

        using var producer = new ProducerBuilder<string, string>(producerConfig)
            .SetKeySerializer(Serializers.Utf8)
            .SetValueSerializer(Serializers.Utf8)
            .SetErrorHandler((_, e) =>
            {
                _fixture.KafkaContainer.Logger.LogError("Error: {Reason}", e.Reason);
            })
            .SetLogHandler((_, logMessage) =>
            {
                _fixture.KafkaContainer.Logger.LogInformation("{Name} sending {Message}", logMessage.Name, logMessage.Message);
            })
            .Build();

        var taskTestResult = _fixture.MicrocksContainerEnsemble
            .MicrocksContainer
            .TestEndpointAsync(testRequest, TestContext.Current.CancellationToken);
        await Task.Delay(750, TestContext.Current.CancellationToken);

        // Act
        for (var i = 0; i < 5; i++)
        {
            producer.Produce("pastry-orders", new Message<string, string>
            {
                Key = Guid.NewGuid().ToString(),
                Value = message
            });
            producer.Flush(TestContext.Current.CancellationToken);
            await Task.Delay(500, TestContext.Current.CancellationToken);
        }

        // Wait for a test result
        var testResult = await taskTestResult;

        // Assert
        Assert.False(testResult.InProgress);
        Assert.False(testResult.Success);
        Assert.Equal(testRequest.TestEndpoint, testResult.TestedEndpoint);

        var testCaseResult = testResult.TestCaseResults.First();
        var testStepResults = testCaseResult.TestStepResults;

        // Minimum 1 message captured
        Assert.NotEmpty(testStepResults);
        // Error message status is missing
        Assert.Contains("object has missing required properties ([\"status\"]", testStepResults.First().Message);

        // Retrieve event messages for the failing test case.
        var events = await _fixture.MicrocksContainerEnsemble.MicrocksContainer
            .GetEventMessagesForTestCaseAsync(testResult, "SUBSCRIBE pastry/orders", TestContext.Current.CancellationToken);
        // We should have at least 4 events.
        Assert.True(events.Count >= 4);

        // Check that all events have the correct message.
        Assert.All(events, e =>
        {
            // Check these are the correct message.
            Assert.NotNull(e.EventMessage);
            Assert.Equal(message, e.EventMessage.Content);
        });
    }

    // Inner fixture to share Kafka and Microcks ensemble between tests
    public sealed class KafkaFixture : IAsyncDisposable
    {
        private const string MicrocksImage = "quay.io/microcks/microcks-uber:1.10.0";

        public INetwork Network { get; }
        public KafkaContainer KafkaContainer { get; }
        public MicrocksContainerEnsemble MicrocksContainerEnsemble { get; }

        public KafkaFixture()
        {
            Network = new NetworkBuilder().Build();

            KafkaContainer = new KafkaBuilder()
                .WithImage("confluentinc/cp-kafka:7.8.0")
                .WithNetwork(Network)
                .WithNetworkAliases("kafka")
                .WithListener("kafka:19092")
                .Build();

            KafkaContainer.StartAsync().GetAwaiter().GetResult();

            MicrocksContainerEnsemble = new MicrocksContainerEnsemble(Network, MicrocksImage)
                .WithMainArtifacts("pastry-orders-asyncapi.yml")
                .WithKafkaConnection(new KafkaConnection($"kafka:19092"));

            MicrocksContainerEnsemble.StartAsync().GetAwaiter().GetResult();
        }

        public async ValueTask DisposeAsync()
        {
            await MicrocksContainerEnsemble.DisposeAsync();
            await KafkaContainer.DisposeAsync();
            await Network.DisposeAsync();
        }
    }

}