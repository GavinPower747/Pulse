using RabbitMQ.Client;

namespace Pulse.Tests.Util.Mocks.RabbitMQ.Models;

public class RabbitMessage
{
    public required string Exchange { get; set; }
    public required string RoutingKey { get; set; }
    public string? Queue { get; set; }
    public bool Mandatory { get; set; }
    public bool Immediate { get; set; }
    public required IBasicProperties BasicProperties { get; set; }
    public required byte[] Body { get; set; }
    public ulong DeliveryTag { get; set; }

    public RabbitMessage Copy()
    {
        return new RabbitMessage
        {
            Exchange = Exchange,
            RoutingKey = RoutingKey,
            Queue = Queue,
            Mandatory = Mandatory,
            Immediate = Immediate,
            BasicProperties = BasicProperties,
            Body = Body,
            DeliveryTag = DeliveryTag,
        };
    }
}
