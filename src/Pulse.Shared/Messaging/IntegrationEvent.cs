using System.Text.Json.Serialization;

namespace Pulse.Shared.Messaging;

public abstract class IntegrationEvent
{
    [JsonIgnore]
    public abstract string EventName { get; }

    [JsonIgnore]
    public abstract string EventVersion { get; }

    [JsonIgnore]
    public abstract Uri Source { get; }

    public IntegrationEvent() { }

    public string GetFullEventName() => $"{EventName}.{EventVersion}";
}