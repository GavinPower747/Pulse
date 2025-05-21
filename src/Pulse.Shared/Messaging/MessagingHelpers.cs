using Microsoft.IdentityModel.Tokens;

namespace Pulse.Shared.Messaging;

public class MessagingHelpers
{
    public static string GetQueueName(IntegrationEvent evt, IConsumer consumer)
        => GetQueueName(evt, consumer.GetType().Name);

    public static string GetQueueName<T>(IntegrationEvent evt, IConsumer<T> consumer) where T : IntegrationEvent
        => GetQueueName(evt, consumer.GetType().Name);

    public static string GetQueueName(IntegrationEvent evt, string consumerName)
        => $"{evt.GetFullEventName()}.queue.{consumerName}";
}