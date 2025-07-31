using System.Text;
using CloudNative.CloudEvents;
using CloudNative.CloudEvents.SystemTextJson;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Pulse.Shared.Messaging;
using Pulse.Shared.Messaging.Resiliance;
using Pulse.Shared.Services;
using Pulse.Tests.Util.Fixtures;
using RabbitMQ.Client;

namespace Pulse.Shared.Tests.Resiliance;

public class ResilianceTests : IDisposable
{
    private readonly RabbitMqFixture _rabbitFixture;
    private readonly AmqpConsumerService<TestEvent> _sut;

    public ResilianceTests()
    {
        _rabbitFixture = new RabbitMqFixture();

        // Set up the specific services needed for this test
        var channelPool = new AmqpChannelPool(
            _rabbitFixture.Connection,
            Substitute.For<ILogger<AmqpChannelPool>>()
        );
        var immediateRetryHandler = new ImmediateRetryHandler(channelPool);
        var exponentialRetryHandler = new ExponentialDelayRetryHandler(channelPool);
        var deadLetterHandler = new DeadLetterHandler(
            Substitute.For<ILogger<DeadLetterHandler>>(),
            channelPool
        );

        _sut = new AmqpConsumerService<TestEvent>(
            _rabbitFixture.Connection,
            new TestConsumer(),
            Substitute.For<ILogger<AmqpConsumerService<TestEvent>>>(),
            immediateRetryHandler,
            exponentialRetryHandler,
            deadLetterHandler
        );

        // Setup infrastructure for our specific test needs
        SetupTestInfrastructureAsync().GetAwaiter().GetResult();
        _sut.StartAsync(CancellationToken.None).GetAwaiter().GetResult();
    }

    public void Dispose()
    {
        _rabbitFixture.Dispose();
    }

    private async Task SetupTestInfrastructureAsync()
    {
        var evtMetadata = IntegrationEvent.GetEventMetadata<TestEvent>();

        await _rabbitFixture.DeclareExchangeAsync(Messaging.Constants.RetryQueue);
        await _rabbitFixture.DeclareExchangeAsync(evtMetadata.GetExchangeName());
        await _rabbitFixture.DeclareQueueAsync(Messaging.Constants.RetryQueue);
        await _rabbitFixture.BindQueueAsync(
            Messaging.Constants.RetryQueue,
            Messaging.Constants.RetryQueue
        );
    }

    [Fact]
    public async Task RetryHandler_Should_Requeue_When_UsingImmediateRetries()
    {
        // Arrange
        TestEvent evt = new() { Id = 1, Name = "Test Event" };
        var evtMetadata = IntegrationEvent.GetEventMetadata<TestEvent>();
        var channel = await _rabbitFixture.GetChannelAsync();
        var properties = new BasicProperties
        {
            ContentType = "application/json",
            DeliveryMode = DeliveryModes.Persistent,
            Headers = new Dictionary<string, object?>
            {
                { Messaging.Constants.Headers.RetryCount, 0 },
            },
        };

        // Act
        await channel.BasicPublishAsync(
            evtMetadata.GetExchangeName(),
            "#",
            true,
            properties,
            WrapMessageAndSerialize(evt, evtMetadata),
            CancellationToken.None
        );

        var messages = channel.GetMessagesOnQueue(
            $"{evtMetadata.GetQueueName(new TestConsumer())}"
        );

        // Assert
        Assert.Equal(2, messages.Count());

        var message = messages.First();
        message.BasicProperties.Headers!.TryGetValue(
            Messaging.Constants.Headers.RetryCount,
            out var retryCount
        );
        Assert.NotNull(retryCount);
        Assert.Equal(1, retryCount);
    }

    [Fact]
    public async Task RetryHandler_Should_SendToDelayQueue_When_UsingImmediateRetriesAndMaxRetriesReached()
    {
        // Arrange
        TestEvent evt = new() { Id = 1, Name = "Test Event" };
        var evtMetadata = IntegrationEvent.GetEventMetadata<TestEvent>();
        var channel = await _rabbitFixture.GetChannelAsync();
        var properties = new BasicProperties
        {
            ContentType = "application/json",
            DeliveryMode = DeliveryModes.Persistent,
            Headers = new Dictionary<string, object?>
            {
                { Messaging.Constants.Headers.RetryCount, 3 },
            },
        };

        // Act
        await channel.BasicPublishAsync(
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
        message.BasicProperties.Headers!.TryGetValue(
            Messaging.Constants.Headers.RetryCount,
            out var retryCount
        );
        Assert.NotNull(retryCount);
        Assert.Equal(4, retryCount);
        Assert.Equal(
            evtMetadata.GetExchangeName(),
            message.BasicProperties.Headers![Messaging.Constants.Headers.RetryRepublishExchange]
        );
        Assert.Equal(
            "#",
            message.BasicProperties.Headers![Messaging.Constants.Headers.RetryRepublishRoutingKey]
        );
    }

    [Fact]
    public async Task RetryHandler_Should_SendToDeadLetterQueue_When_UsingExponentialRetriesAndMaxRetriesReached()
    {
        // Arrange
        TestEvent evt = new() { Id = 1, Name = "Test Event" };
        var evtMetadata = IntegrationEvent.GetEventMetadata<TestEvent>();
        var channel = await _rabbitFixture.GetChannelAsync();
        var properties = new BasicProperties
        {
            ContentType = "application/json",
            DeliveryMode = DeliveryModes.Persistent,
            Headers = new Dictionary<string, object?>
            {
                { Messaging.Constants.Headers.RetryCount, 6 },
            },
        };

        // Act
        await channel.BasicPublishAsync(
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

    private static ReadOnlyMemory<byte> WrapMessageAndSerialize(
        object? message,
        EventMetadata metadata
    )
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
            Data = message,
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
