using Microsoft.EntityFrameworkCore;
using Pulse.Followers.Domain;

namespace Pulse.Followers.Data;

internal class FollowingContext(DbContextOptions<FollowingContext> options) : DbContext(options)
{
    public DbSet<Following> Followings { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new FollowingMap());
    }
}
