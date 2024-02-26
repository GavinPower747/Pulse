using System.Collections.Generic;
using Pulse.Shared.Domain;

namespace Pulse.Shared;

public class Entity
{
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents;

    private readonly List<IDomainEvent> _domainEvents = [];

    public void AddDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);

    public void ClearDomainEvents() => _domainEvents.Clear();
}
