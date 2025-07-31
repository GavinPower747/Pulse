using Microsoft.EntityFrameworkCore;
using Pulse.Posts.Domain;

namespace Pulse.Posts.Data;

internal class AttachmentContext(DbContextOptions<AttachmentContext> options) : DbContext(options)
{
    public DbSet<AttachmentMetadata> AttachmentMetadata { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new AttachmentMetadataMap());
    }
}
