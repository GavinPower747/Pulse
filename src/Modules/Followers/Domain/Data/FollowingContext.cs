using MediatR;
using Microsoft.EntityFrameworkCore;
using Pulse.Followers.Domain;
using Pulse.Shared.Data;

namespace Pulse.Followers.Data;

internal class FollowingContext(DbContextOptions<FollowingContext> options, IMediator mediator)
    : DomainContext(options, mediator)
{
    public DbSet<Following> Followings { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new FollowingMap());
    }
}
