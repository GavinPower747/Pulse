namespace Pulse.Shared.Messaging;

public class Constants
{
    public const string RetryQueue = "pulse.retry";

    public class Headers
    {
        public const string RetryCount = "x-retry-count";
        public const string RetryDelay = "x-message-ttl";
        public const string RetryRepublishExchange = "x-dead-letter-exchange";
        public const string RetryRepublishRoutingKey = "x-dead-letter-routing-key";
    }
}
