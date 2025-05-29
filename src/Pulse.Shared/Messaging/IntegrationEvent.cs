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

    public IntegrationEvent() { }

    public string GetFullEventName() => $"{EventName}.{EventVersion}";

    internal static EventMetadata GetEventMetadata<T>() where T : IntegrationEvent
        => GetEventMetadata(typeof(T));

    internal static EventMetadata GetEventMetadata(Type eventType)
        => EventMetadataCache.GetOrAdd(
            eventType,
            type =>
            {
                var evt = (IntegrationEvent) RuntimeHelpers.GetUninitializedObject(type);
                return new EventMetadata(
                    evt.GetFullEventName(),
                    evt.EventName,
                    evt.EventVersion,
                    evt.Source
                );
            }
        );
}

internal class EventMetadata(string fullName, string eventName, string eventVersion, Uri source)
{
    internal string FullName { get; } = fullName;
    internal string EventName { get; } = eventName;
    internal string EventVersion { get; } = eventVersion;
    internal Uri Source { get; } = source;

    internal string GetExchangeName() => FullName;
    internal string GetQueueName(IConsumer consumer) => GetQueueName(consumer.GetType().Name);
    internal string GetQueueName<T>(IConsumer<T> consumer) where T : IntegrationEvent => GetQueueName(consumer.GetType().Name);
    private string GetQueueName(string consumerName) => $"{FullName}.queue.{consumerName}";
}
