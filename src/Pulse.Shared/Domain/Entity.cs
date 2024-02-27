using MediatR;

namespace Pulse.Shared;

public class Entity
{
    public IReadOnlyList<INotification> DomainEvents => _domainEvents;

    private readonly List<INotification> _domainEvents = [];

    public void AddDomainEvent(INotification domainEvent) => _domainEvents.Add(domainEvent);

    public void ClearDomainEvents() => _domainEvents.Clear();
}
