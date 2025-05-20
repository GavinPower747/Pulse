using System.Text;

using CloudNative.CloudEvents;
using CloudNative.CloudEvents.SystemTextJson;

using Microsoft.Extensions.Logging;

using RabbitMQ.Client;

namespace Pulse.Shared.Messaging;

public class AmqpPublisher(ILogger<AmqpPublisher> logger, AmqpChannelPool channelPool) : IProducer
{
    private readonly ILogger<AmqpPublisher> _logger = logger;
    private readonly AmqpChannelPool _channelPool = channelPool;

    public const string MessageContentType = "application/json";

    public async Task Publish(IntegrationEvent evt, CancellationToken token = default)
    {
        var eventType = $"{evt.EventName}.{evt.EventVersion}";

        var eventId = Guid.NewGuid().ToString();

        var evtWrapper = new CloudEvent()
        {
            Type = eventType,
            Source = evt.Source,
            Time = DateTimeOffset.UtcNow,
            DataContentType = MessageContentType,
            Id = eventId,
            Data = evt,
        };

        var evtFormatter = new JsonEventFormatter();
        var json = evtFormatter.ConvertToJsonElement(evtWrapper).ToString();

        await _channelPool.UseChannel(async channel =>
        {
            BasicProperties properties = new()
            {
                ContentType = MessageContentType,
                DeliveryMode = DeliveryModes.Persistent
            };

            await channel.BasicPublishAsync(
                exchange: eventType,
                routingKey: "#",
                true,
                properties,
                Encoding.UTF8.GetBytes(json),
                token
            );
        });
    }
}
