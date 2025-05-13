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

using Testcontainers.RabbitMq;
using DotNet.Testcontainers.Networks;
using DotNet.Testcontainers.Builders;
using System;
using System.Threading;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Microcks.Testcontainers.Connection;

namespace Microcks.Testcontainers.Tests.Async.Amqp;

public class MicrocksAsyncAmqpFeatureTest : IAsyncLifetime
{
    private RabbitMqContainer _rabbitMqContainer;
    private MicrocksContainerEnsemble _ensemble;
    private INetwork _network;

    public async ValueTask InitializeAsync()
    {
        _network = new NetworkBuilder().Build();
        _rabbitMqContainer = new RabbitMqBuilder()
            .WithImage("rabbitmq:3.13-management-alpine")
            .WithNetwork(_network)
            .WithNetworkAliases("rabbitmq")
            .Build();

        await _rabbitMqContainer.StartAsync()
            .ConfigureAwait(false);
        await _rabbitMqContainer.ExecAsync(["rabbitmqctl", "add_user", "test", "test"]);
        await _rabbitMqContainer.ExecAsync(["rabbitmqctl", "set_permissions", "-p", "/", "test", ".*", ".*", ".*"]);

        _ensemble = new MicrocksContainerEnsemble(_network, "quay.io/microcks/microcks-uber")
            .WithMainArtifacts("pastry-orders-asyncapi.yml")
            .WithAsyncFeature()
            .WithAmqpConnection(new GenericConnection("rabbitmq:5672", "test", "test"));
        await _ensemble.StartAsync();
    }

    public async ValueTask DisposeAsync()
    {
        if (_ensemble != null)
            await _ensemble.DisposeAsync();
        if (_rabbitMqContainer != null)
            await _rabbitMqContainer.DisposeAsync();
        if (_network != null)
            await _network.DisposeAsync();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task ShouldReceiveAmqpMockMessageWhenMicrocksAsyncMinonEmits()
    {
        const string expectedMessage = "{\"id\":\"4dab240d-7847-4e25-8ef3-1530687650c8\",\"customerId\":\"fe1088b3-9f30-4dc1-a93d-7b74f0a072b9\",\"status\":\"VALIDATED\",\"productQuantities\":[{\"quantity\":2,\"pastryName\":\"Croissant\"},{\"quantity\":1,\"pastryName\":\"Millefeuille\"}]}";
        // Construction du nom de la destination AMQP comme dans le test Kafka
        var amqpDestination = "PastryordersAPI-0.1.0-pastry/orders";

        var factory = new ConnectionFactory
        {
            HostName = _rabbitMqContainer.Hostname,
            Port = _rabbitMqContainer.GetMappedPublicPort(5672),
            UserName = "test",
            Password = "test"
        };

        string receivedMessage = null;
        await using var connection = await factory.CreateConnectionAsync(cancellationToken: TestContext.Current.CancellationToken);
        await using var channel = await connection.CreateChannelAsync(cancellationToken: TestContext.Current.CancellationToken);

        await channel.ExchangeDeclareAsync(amqpDestination, "topic", false, cancellationToken: TestContext.Current.CancellationToken);
        var queueDeclareResult = await channel.QueueDeclareAsync(cancellationToken: TestContext.Current.CancellationToken);
        var queueName = queueDeclareResult.QueueName;
        await channel.QueueBindAsync(queueName, amqpDestination, "#", cancellationToken: TestContext.Current.CancellationToken);

        var consumer = new AsyncEventingBasicConsumer(channel);
        var messageReceived = new ManualResetEventSlim(false);

        consumer.ReceivedAsync += async (model, ea) =>
        {
            receivedMessage = System.Text.Encoding.UTF8.GetString(ea.Body.ToArray());
            await channel.BasicAckAsync(ea.DeliveryTag, false, cancellationToken: TestContext.Current.CancellationToken);
            messageReceived.Set();
        };

        await channel.BasicConsumeAsync(queue: queueName, autoAck: false, consumer: consumer, cancellationToken: TestContext.Current.CancellationToken);

        // Attendre le message (timeout 5s)
        messageReceived.Wait(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        Assert.NotNull(receivedMessage);
        Assert.True(receivedMessage.Length > 1);
        Assert.Equal(expectedMessage, receivedMessage);
    }
}
