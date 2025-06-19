using System.Collections;
using System.Collections.Concurrent;

namespace Pulse.Tests.Util.Mocks.RabbitMQ.Models;

public abstract class Exchange(string name, string type)
{
    public string Name { get; } = name;
    public string Type { get; } = type;
    public bool IsDurable { get; set; }
    public bool AutoDelete { get; set; }
    public IDictionary Arguments = new Dictionary<string, object>();

    public ConcurrentDictionary<string, IList<Queue>> QueueBindings = new();

    public abstract void BindQueue(string bindingKey, Queue queue);

    public abstract void UnbindQueue(string bindingKey, Queue queue);

    protected abstract IEnumerable<Queue> GetQueues(RabbitMessage message);

    public bool PublishMessage(RabbitMessage message)
    {
        var queues = GetQueues(message);
        if (queues?.Any() != true)
        {
            return false;
        }

        foreach (var queue in queues)
        {
            queue.PublishMessage(message);
        }

        return true;
    }
}