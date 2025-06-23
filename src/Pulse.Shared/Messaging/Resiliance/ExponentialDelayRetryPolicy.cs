using System.Reflection.Metadata;

using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Pulse.Shared.Messaging.Resiliance;

internal class ExponentialDelayRetryHandler(AmqpChannelPool channelPool) : IFailureHandler
{
    private readonly AmqpChannelPool _channelPool = channelPool;

    public async Task HandleFailure(BasicDeliverEventArgs args, IntegrationEvent evt, CancellationToken ct = default)
    {
        await _channelPool.UseChannel(async channel =>
        {
            args.BasicProperties.Headers!.TryGetValue(Constants.Headers.RetryCount, out var retryCountRaw);
            var retryCount = retryCountRaw is int count ? count : 0;

            var delay = TimeSpan.FromSeconds(Math.Pow(2, retryCount));

            await _channelPool.UseChannel(async channel =>
            {
                var basicProperties = new BasicProperties(args.BasicProperties);
                var evtMetadata = IntegrationEvent.GetEventMetadata(evt.GetType());

                basicProperties.Headers ??= new Dictionary<string, object?>();

                // Set it so that when the ttl of the message (i.e. the delay) expires, it will be "dead lettered" (republished in our case) to the original exchange and thus retried
                basicProperties.Expiration = delay.TotalMilliseconds.ToString();
                basicProperties.Headers[Constants.Headers.RetryRepublishExchange] = evtMetadata.GetExchangeName();
                basicProperties.Headers[Constants.Headers.RetryRepublishRoutingKey] = "#";

                await channel.BasicPublishAsync(
                    exchange: Constants.RetryQueue,
                    routingKey: "#",
                    mandatory: true,
                    basicProperties: basicProperties,
                    body: args.Body,
                    cancellationToken: ct
                );
            });
        });
    }

}
