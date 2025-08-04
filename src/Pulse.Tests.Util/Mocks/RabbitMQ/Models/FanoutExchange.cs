using RabbitMQ.Client;

namespace Pulse.Tests.Util.Mocks.RabbitMQ.Models;

public class FanoutExchange(string name) : Exchange(name, ExchangeType.Fanout)
{
    private readonly IList<Queue> _queues = [];

    public override void BindQueue(string bindingKey, Queue queue)
    {
        if (_queues.Any(q => q.Name == queue.Name))
        {
            // TODO: Throw Exception?
            return;
        }

        _queues.Add(queue);
    }

    public override void UnbindQueue(string bindingKey, Queue queue)
    {
        _queues.Remove(queue);
    }

    protected override IEnumerable<Queue> GetQueues(RabbitMessage message)
    {
        // Messages can always be routed, unless there are no bound Queues.
        if (_queues.Count == 0)
        {
            return Enumerable.Empty<Queue>();
        }

        return _queues;
    }
}
