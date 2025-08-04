using Microsoft.Extensions.Logging;
using NSubstitute;
using Pulse.Shared.Messaging;
using Pulse.Tests.Util.Mocks.RabbitMQ;
using Pulse.Tests.Util.Mocks.RabbitMQ.Models;
using RabbitMQ.Client;
using Xunit;

namespace Pulse.Tests.Util.Fixtures;

/// <summary>
/// Basic RabbitMQ test fixture providing fake RabbitMQ infrastructure.
/// Offers a composition-based approach for testing with minimal dependencies.
/// </summary>
public class RabbitMqFixture : IDisposable
{
    public RabbitServer RabbitServer { get; }
    public FakeConnection Connection { get; }

    private bool _disposed = false;

    public RabbitMqFixture()
    {
        RabbitServer = new RabbitServer();
        Connection = new FakeConnection(RabbitServer);
    }

    /// <summary>
    /// Gets a fake channel for testing message publishing and inspection.
    /// </summary>
    /// <returns>FakeChannel instance</returns>
    public async Task<FakeChannel> GetChannelAsync()
    {
        return await Connection.CreateChannelAsync() as FakeChannel
            ?? throw new InvalidOperationException("Expected FakeChannel but got different type");
    }

    public IProducer GetProducer()
    {
        return new AmqpPublisher(
            new AmqpChannelPool(Connection, Substitute.For<ILogger<AmqpChannelPool>>())
        );
    }

    public IEnumerable<RabbitMessage> GetMessagesOnQueue(string queueName)
    {
        var channel = (FakeChannel)Connection.CreateChannel();
        return channel.GetMessagesOnQueue(queueName);
    }

    public IEnumerable<RabbitMessage> GetMessagesForEvent<T>(string consumerName)
        where T : IntegrationEvent
    {
        var metadata = IntegrationEvent.GetEventMetadata<T>();
        return GetMessagesOnQueue(metadata.GetQueueName(consumerName));
    }

    public async Task DeclareForEvent<T>(string consumerName)
        where T : IntegrationEvent
    {
        var metadata = IntegrationEvent.GetEventMetadata<T>();
        var channel = await Connection.CreateChannelAsync();

        try
        {
            await channel.ExchangeDeclareAsync(
                metadata.GetExchangeName(),
                ExchangeType.Fanout,
                true,
                false
            );

            await channel.QueueDeclareAsync(
                metadata.GetQueueName(consumerName),
                true,
                false,
                false
            );

            await channel.QueueBindAsync(
                metadata.GetQueueName(consumerName),
                metadata.GetExchangeName(),
                string.Empty
            );
        }
        catch
        {
            // Ignore exceptions, they were probably declared in parallel tests
        }
    }

    public static string GetQueueNameForEvent<T>(string consumerName)
        where T : IntegrationEvent
    {
        var metadata = IntegrationEvent.GetEventMetadata<T>();
        return metadata.GetQueueName(consumerName);
    }

    /// <summary>
    /// Helper method to declare an exchange.
    /// </summary>
    /// <param name="exchangeName">Name of the exchange</param>
    /// <param name="exchangeType">Type of the exchange (fanout, direct, topic, headers)</param>
    /// <param name="durable">Whether the exchange should survive server restarts</param>
    /// <param name="autoDelete">Whether the exchange should be auto-deleted when no longer used</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public async Task DeclareExchangeAsync(
        string exchangeName,
        string exchangeType = ExchangeType.Fanout,
        bool durable = true,
        bool autoDelete = false,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            using var channel = await Connection.CreateChannelAsync();
            await channel.ExchangeDeclareAsync(
                exchangeName,
                exchangeType,
                durable,
                autoDelete,
                null,
                cancellationToken: cancellationToken
            );
        }
        catch
        {
            // Ignore exceptions, they were probably declared in parallel tests
        }
    }

    /// <summary>
    /// Helper method to declare a queue.
    /// </summary>
    /// <param name="queueName">Name of the queue</param>
    /// <param name="durable">Whether the queue should survive server restarts</param>
    /// <param name="exclusive">Whether the queue should be exclusive to this connection</param>
    /// <param name="autoDelete">Whether the queue should be auto-deleted when no longer used</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public async Task DeclareQueueAsync(
        string queueName,
        bool durable = true,
        bool exclusive = false,
        bool autoDelete = false,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            using var channel = await Connection.CreateChannelAsync();
            await channel.QueueDeclareAsync(
                queueName,
                durable,
                exclusive,
                autoDelete,
                null,
                false,
                cancellationToken
            );
        }
        catch
        {
            // Ignore exceptions, they were probably declared in parallel tests
        }
    }

    /// <summary>
    /// Helper method to bind a queue to an exchange.
    /// </summary>
    /// <param name="queueName">Name of the queue</param>
    /// <param name="exchangeName">Name of the exchange</param>
    /// <param name="routingKey">Routing key for the binding</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public async Task BindQueueAsync(
        string queueName,
        string exchangeName,
        string routingKey = "#",
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            using var channel = await Connection.CreateChannelAsync();
            await channel.QueueBindAsync(
                queueName,
                exchangeName,
                routingKey,
                null,
                false,
                cancellationToken
            );
        }
        catch
        {
            // Ignore exceptions, they were probably declared in parallel tests
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            // Clean up resources if needed
            _disposed = true;
        }
    }
}

[CollectionDefinition("RabbitMq")]
public class RabbitMqCollection : ICollectionFixture<RabbitMqFixture> { }
