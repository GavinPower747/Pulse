using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Pulse.Shared.Messaging.Resiliance;

internal class ImmediateRetryHandler(AmqpChannelPool channelPool) : IFailureHandler
{
    private readonly AmqpChannelPool _channelPool = channelPool;

    public async Task HandleFailure(
        BasicDeliverEventArgs args,
        IntegrationEvent evt,
        CancellationToken ct = default
    )
    {
        await _channelPool.UseChannel(async channel =>
        {
            var metadata = IntegrationEvent.GetEventMetadata(evt.GetType());

            await channel.BasicPublishAsync(
                exchange: evt.GetFullEventName(),
                routingKey: "#",
                true,
                new BasicProperties(args.BasicProperties),
                args.Body,
                ct
            );
        });
    }
}
