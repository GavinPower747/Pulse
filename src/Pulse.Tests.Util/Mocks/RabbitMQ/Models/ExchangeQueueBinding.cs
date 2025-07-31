namespace Pulse.Tests.Util.Mocks.RabbitMQ.Models;

public class ExchangeQueueBinding
{
    public required string RoutingKey { get; set; }

    public required Exchange Exchange { get; set; }

    public required Queue Queue { get; set; }

    public string Key
    {
        get { return string.Format("{0}|{1}|{2}", Exchange.Name, RoutingKey, Queue.Name); }
    }
}
