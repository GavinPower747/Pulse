using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Pulse.Shared.Data;

public class DomainContext(DbContextOptions options, IMediator mediator) : DbContext(options)
{
    private readonly IMediator _mediator = mediator;

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var domainEntities = ChangeTracker
            .Entries<Entity>()
            .Where(x => x.Entity.DomainEvents.Any())
            .ToList();

        domainEntities.ForEach(entity =>
        {
            var events = entity.Entity.DomainEvents;

            foreach (var domainEvent in events)
            {
                _mediator.Publish(domainEvent);
            }

            entity.Entity.ClearDomainEvents();
        });

        return base.SaveChangesAsync(cancellationToken);
    }
}
