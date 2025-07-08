

using Microsoft.Extensions.Logging;

using RabbitMQ.Client;

using RabbitMQ.Client.Events;

namespace Pulse.Shared.Messaging.Resiliance;

internal class DeadLetterHandler(ILogger<DeadLetterHandler> logger, AmqpChannelPool channelPool) : IFailureHandler
{
    private readonly ILogger<DeadLetterHandler> _logger = logger;
    private readonly AmqpChannelPool _channelPool = channelPool;

    public async Task HandleFailure(BasicDeliverEventArgs args, IntegrationEvent evt, CancellationToken ct = default)
    {
        _logger.LogWarning("Message failed to process, sending to dead letter queue");

        await _channelPool.UseChannel(async channel =>
        {
            await channel.ExchangeDeclareAsync(Constants.DeadLetterQueue, ExchangeType.Fanout, true, false, null, cancellationToken: ct);
            await channel.QueueDeclareAsync(Constants.DeadLetterQueue, true, false, false, null, cancellationToken: ct);
            await channel.QueueBindAsync(Constants.DeadLetterQueue, Constants.DeadLetterQueue, "", null, cancellationToken: ct);

            await channel.BasicPublishAsync(
                exchange: Constants.DeadLetterQueue,
                routingKey: "",
                mandatory: false,
                basicProperties: new BasicProperties(args.BasicProperties),
                body: args.Body,
                cancellationToken: ct
            );
        });        
    }

}