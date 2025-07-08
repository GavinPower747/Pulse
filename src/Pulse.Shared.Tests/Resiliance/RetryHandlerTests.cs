using RabbitMQ.Client;
using Pulse.Tests.Util.Mocks.RabbitMQ;
using Pulse.Shared.Messaging;
using NSubstitute;
using Microsoft.Extensions.Logging;
using Pulse.Shared.Services;
using CloudNative.CloudEvents;
using CloudNative.CloudEvents.SystemTextJson;
using System.Text;
using Pulse.Shared.Messaging.Resiliance;

namespace Pulse.Shared.Tests.Resiliance;

public class ResilianceTests
{
    private readonly RabbitServer _rabbitServer;
    private readonly FakeConnection _connection;
    private readonly AmqpConsumerService<TestEvent> _sut;

    public ResilianceTests()
    {
        _rabbitServer = new();
        _connection = new(_rabbitServer);
        var amqpPool = new AmqpChannelPool(_connection, Substitute.For<ILogger<AmqpChannelPool>>());
        var immediateRetryHandler = new ImmediateRetryHandler(amqpPool);
        var exponentialRetryHandler = new ExponentialDelayRetryHandler(amqpPool);
        var deadLetterHandler = new DeadLetterHandler(Substitute.For<ILogger<DeadLetterHandler>>(), amqpPool);
        _sut = new AmqpConsumerService<TestEvent>(
            _connection,
            new TestConsumer(),
            Substitute.For<ILogger<AmqpConsumerService<TestEvent>>>(),
            immediateRetryHandler,
            exponentialRetryHandler,
            deadLetterHandler
        );

        using var channel = _connection.CreateChannelAsync().GetAwaiter().GetResult();
        var evtMetadata = IntegrationEvent.GetEventMetadata<TestEvent>();

        channel.ExchangeDeclareAsync(Messaging.Constants.RetryQueue, ExchangeType.Fanout, true, false, null).GetAwaiter().GetResult();
        channel.ExchangeDeclareAsync(evtMetadata.GetExchangeName(), ExchangeType.Fanout, true, false, null).GetAwaiter().GetResult();

        channel.QueueDeclareAsync(
            Messaging.Constants.RetryQueue,
            true,
            false,
            false,
            null,
            false,
            CancellationToken.None
        ).GetAwaiter().GetResult();
        
        channel.QueueBindAsync(
            Messaging.Constants.RetryQueue,
            Messaging.Constants.RetryQueue,
            "#",
            null,
            false,
            CancellationToken.None
        ).GetAwaiter().GetResult();

        _sut.StartAsync(CancellationToken.None).GetAwaiter().GetResult();
    }

    [Fact]
    public async Task RetryHandler_Should_Requeue_When_UsingImmediateRetries()
    {
        // Arrange
        TestEvent evt = new() { Id = 1, Name = "Test Event" };
        var evtMetadata = IntegrationEvent.GetEventMetadata<TestEvent>();
        var channel = await _connection.CreateChannelAsync() as FakeChannel;
        var properties = new BasicProperties
        {
            ContentType = "application/json",
            DeliveryMode = DeliveryModes.Persistent,
            Headers = new Dictionary<string, object?>
            {
                { Messaging.Constants.Headers.RetryCount, 0 }
            }
        };

        // Act
        await channel!.BasicPublishAsync(
            evtMetadata.GetExchangeName(),
            "#",
            true,
            properties,
            WrapMessageAndSerialize(evt, evtMetadata),
            CancellationToken.None
        );

        var messages = channel.GetMessagesOnQueue($"{evtMetadata.GetQueueName(new TestConsumer())}");

        // Assert
        Assert.Equal(2, messages.Count());

        var message = messages.First();
        message.BasicProperties.Headers!.TryGetValue(Messaging.Constants.Headers.RetryCount, out var retryCount);
        Assert.NotNull(retryCount);
        Assert.Equal(1, retryCount);
    }

    [Fact]
    public async Task RetryHandler_Should_SendToDelayQueue_When_UsingImmediateRetriesAndMaxRetriesReached()
    {
        // Arrange
        TestEvent evt = new() { Id = 1, Name = "Test Event" };
        var evtMetadata = IntegrationEvent.GetEventMetadata<TestEvent>();
        var channel = await _connection.CreateChannelAsync() as FakeChannel;
        var properties = new BasicProperties
        {
            ContentType = "application/json",
            DeliveryMode = DeliveryModes.Persistent,
            Headers = new Dictionary<string, object?>
            {
                { Messaging.Constants.Headers.RetryCount, 3 }
            }
        };

        // Act
        await channel!.BasicPublishAsync(
            evtMetadata.GetExchangeName(),
            "#",
            true,
            properties,
            WrapMessageAndSerialize(evt, evtMetadata),
            CancellationToken.None
        );

        var delayMessages = channel.GetMessagesOnQueue(Messaging.Constants.RetryQueue);

        // Assert
        Assert.Single(delayMessages);

        var message = delayMessages.First();
        message.BasicProperties.Headers!.TryGetValue(Messaging.Constants.Headers.RetryCount, out var retryCount);
        Assert.NotNull(retryCount);
        Assert.Equal(4, retryCount);
        Assert.Equal(evtMetadata.GetExchangeName(), message.BasicProperties.Headers![Messaging.Constants.Headers.RetryRepublishExchange]);
        Assert.Equal("#", message.BasicProperties.Headers![Messaging.Constants.Headers.RetryRepublishRoutingKey]);
    }

    [Fact]
    public async Task RetryHandler_Should_SendToDeadLetterQueue_When_UsingExponentialRetriesAndMaxRetriesReached()
    { 
        // Arrange
        TestEvent evt = new() { Id = 1, Name = "Test Event" };
        var evtMetadata = IntegrationEvent.GetEventMetadata<TestEvent>();
        var channel = await _connection.CreateChannelAsync() as FakeChannel;
        var properties = new BasicProperties
        {
            ContentType = "application/json",
            DeliveryMode = DeliveryModes.Persistent,
            Headers = new Dictionary<string, object?>
            {
                { Messaging.Constants.Headers.RetryCount, 6 }
            }
        };

        // Act
        await channel!.BasicPublishAsync(
            evtMetadata.GetExchangeName(),
            "#",
            true,
            properties,
            WrapMessageAndSerialize(evt, evtMetadata),
            CancellationToken.None
        );

        var deadLetterMessages = channel.GetMessagesOnQueue(Messaging.Constants.DeadLetterQueue);

        // Assert
        Assert.Single(deadLetterMessages);
    }

    private static ReadOnlyMemory<byte> WrapMessageAndSerialize(object? message, EventMetadata metadata)
    {
        if (message is null)
            return ReadOnlyMemory<byte>.Empty;

        var jsonFormatter = new JsonEventFormatter();
        var cloudEvent = new CloudEvent
        {
            Type = metadata.FullName,
            Source = metadata.Source,
            DataContentType = "application/json",
            Id = Guid.NewGuid().ToString(),
            Data = message
        };

        return Encoding.UTF8.GetBytes(jsonFormatter.ConvertToJsonElement(cloudEvent).ToString());
    }
}

public class TestConsumer : IConsumer<TestEvent>
{
    public Task Consume(TestEvent evt, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }
}

public class TestEvent : IntegrationEvent
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public override string EventName => "test.event";

    public override string EventVersion => "v1";

    public override Uri Source => new("pulse://test.event");

}
