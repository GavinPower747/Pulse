using System.Collections.Concurrent;
using Pulse.Tests.Util.Mocks.RabbitMQ.Models;

namespace Pulse.Tests.Util.Mocks.RabbitMQ;

public class RabbitServer
{
    public ConcurrentDictionary<string, Exchange> Exchanges { get; } = new();

    public ConcurrentDictionary<string, Queue> Queues { get; } = new();

    public DirectExchange DefaultExchange => (DirectExchange)Exchanges[string.Empty];

    public RabbitServer()
    {
        InitializeDefaultExchange();
    }

    public void Reset()
    {
        Exchanges.Clear();
        Queues.Clear();

        // Need to re-initialize the default exchange.
        InitializeDefaultExchange();
    }

    private void InitializeDefaultExchange()
    {
        // https://www.rabbitmq.com/tutorials/amqp-concepts.html#exchange-default
        Exchanges[string.Empty] = new DirectExchange(string.Empty);
    }
}
