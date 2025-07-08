using RabbitMQ.Client;

namespace Pulse.Tests.Util.Mocks.RabbitMQ.Models;

public class ExchangeFactory
{
    public static Exchange GetExchange(string name, string type) => type switch
    {
        ExchangeType.Direct => new DirectExchange(name),
        ExchangeType.Fanout => new FanoutExchange(name),
        _ => throw new NotSupportedException($"Exchange type '{type}' is not supported."),
    };
}