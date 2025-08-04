using System.Collections.Concurrent;
using RabbitMQ.Client;

namespace Pulse.Tests.Util.Mocks.RabbitMQ.Models;

public class Queue
{
    private readonly ConcurrentDictionary<string, IAsyncBasicConsumer> _consumers = new();
    private readonly ConcurrentQueue<string> _consumersQueue = new();

    public required string Name { get; set; }

    public bool IsDurable { get; set; }

    public bool IsExclusive { get; set; }

    public bool IsAutoDelete { get; set; }

    public IDictionary<string, object> Arguments = new Dictionary<string, object>();

    public ConcurrentQueue<RabbitMessage> Messages = new();

    public void PublishMessage(RabbitMessage message)
    {
        message.Queue = Name;

        Messages.Enqueue(message);

        DeliverMessage(message);
    }

    public void AddConsumer(string tag, IAsyncBasicConsumer consumer)
    {
        _consumers[tag] = consumer;
        RebuildConsumerQueue();

        // There may have been messages in the queue before a consumer subscribed.
        // Deliver those messages now.
        // NOTE: The use of foreach is intentional here. We don't want to dequeue the messages.
        foreach (var message in Messages)
        {
            // DeliverMessage handles message delivery in a round-robin fashion.
            DeliverMessage(message);
        }
    }

    public void RemoveConsumer(string tag)
    {
        if (_consumers.TryRemove(tag, out var _))
        {
            RebuildConsumerQueue();
        }
    }

    private void RebuildConsumerQueue()
    {
        while (_consumersQueue.TryDequeue(out var _))
        {
            // Intentionally do nothing.
        }

        foreach (var consumer in _consumers.Keys)
        {
            _consumersQueue.Enqueue(consumer);
        }
    }

    private void DeliverMessage(RabbitMessage message)
    {
        // Simulates round-robin message delivery by dequeuing a consumer, delivering a message to it, and re-enqueuing it.
        if (_consumersQueue.TryDequeue(out var consumerName))
        {
            if (_consumers.TryGetValue(consumerName, out var consumer))
            {
                consumer
                    .HandleBasicDeliverAsync(
                        consumerName,
                        message.DeliveryTag,
                        false,
                        message.Exchange,
                        message.RoutingKey,
                        message.BasicProperties,
                        message.Body
                    )
                    .Wait();
            }

            _consumersQueue.Enqueue(consumerName);
        }
    }
}
