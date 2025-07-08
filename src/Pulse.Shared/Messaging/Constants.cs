namespace Pulse.Shared.Messaging;

public class Constants
{
    public const string RetryQueue = "pulse.retry";
    public const string DeadLetterQueue = "pulse.dlq";

    public class Headers
    {
        public const string RetryCount = "x-retry-count";
        public const string RetryDelay = "x-message-ttl";
        public const string RetryRepublishExchange = "x-dead-letter-exchange";
        public const string RetryRepublishRoutingKey = "x-dead-letter-routing-key";
        public const string FirstFailedAt = "x-first-failed-at";
        public const string FirstFailureReason = "x-first-failure-reason";
        public const string FirstFailureException = "x-first-failure-exception";
        public const string LastFailedAt = "x-last-failed-at";
        public const string LastFailureReason = "x-last-failure-reason";
        public const string LastFailureException = "x-last-failure-exception";
    }
}
