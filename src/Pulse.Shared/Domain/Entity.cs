using System.Text.Json.Serialization;
using MediatR;

namespace Pulse.Shared;

public class Entity
{
    [JsonIgnore]
    public IReadOnlyList<INotification> DomainEvents => _domainEvents;

    [JsonIgnore]
    private readonly List<INotification> _domainEvents = [];

    public void AddDomainEvent(INotification domainEvent) => _domainEvents.Add(domainEvent);

    public void ClearDomainEvents() => _domainEvents.Clear();
}
