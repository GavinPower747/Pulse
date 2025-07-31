using System.Collections.Concurrent;
using RabbitMQ.Client;

namespace Pulse.Tests.Util.Mocks.RabbitMQ.Models;

public class DirectExchange(string name) : Exchange(name, ExchangeType.Direct)
{
    public ConcurrentDictionary<string, IList<Queue>> Bindings { get; } =
        new ConcurrentDictionary<string, IList<Queue>>();

    public override void BindQueue(string bindingKey, Queue queue)
    {
        // No binding has been established between this binding key and queue.
        if (!Bindings.TryGetValue(bindingKey, out var queues))
        {
            queues = [];
        }
        // Handle the case when the queue is being re-bound (duplicate binding).
        else if (queues.Any(q => q.Name == queue.Name))
        {
            throw new InvalidOperationException(
                $"Queue '{queue.Name}' was already bound with the binding key '{bindingKey}'."
            );
        }

        queues.Add(queue);
        Bindings[bindingKey] = queues;
    }

    public override void UnbindQueue(string bindingKey, Queue queue)
    {
        if (!Bindings.TryGetValue(bindingKey, out var bindings))
        {
            // TODO: Silently fail?
            return;
        }

        bindings.Remove(queue);

        // If there are no more bindings for the specified binding key, remove the KVP.
        if (bindings.Count == 0)
        {
            Bindings.TryRemove(bindingKey, out var _);
        }
    }

    protected override IEnumerable<Queue> GetQueues(RabbitMessage message) =>
        !Bindings.TryGetValue(message.RoutingKey, out var queues)
            ? Enumerable.Empty<Queue>()
            : queues;
}
