using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace Pulse.Shared.Messaging;

public abstract class IntegrationEvent
{
    internal static ConcurrentDictionary<Type, EventMetadata> EventMetadataCache { get; } = new();

    [JsonIgnore]
    public abstract string EventName { get; }

    [JsonIgnore]
    public abstract string EventVersion { get; }

    [JsonIgnore]
    public abstract Uri Source { get; }

    public string GetFullEventName() => $"{EventName}.{EventVersion}";

    internal static EventMetadata GetEventMetadata<T>()
        where T : IntegrationEvent => GetEventMetadata(typeof(T));

    internal static EventMetadata GetEventMetadata(Type eventType) =>
        EventMetadataCache.GetOrAdd(
            eventType,
            type =>
            {
                var evt = (IntegrationEvent)RuntimeHelpers.GetUninitializedObject(type);
                return new EventMetadata(
                    evt.GetFullEventName(),
                    evt.EventName,
                    evt.EventVersion,
                    evt.Source
                );
            }
        );
}

internal record EventMetadata(string FullName, string EventName, string EventVersion, Uri Source)
{
    internal string GetExchangeName() => FullName;

    internal string GetQueueName(IConsumer consumer) => GetQueueName(consumer.GetType().Name);

    internal string GetQueueName<T>(IConsumer<T> consumer)
        where T : IntegrationEvent => GetQueueName(consumer.GetType().Name);

    private string GetQueueName(string consumerName) => $"{FullName}.queue.{consumerName}";

    private string GetDLQName(string consumerName) =>
        $"{GetQueueName(consumerName)}.dlq";
}
