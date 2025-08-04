using Microsoft.EntityFrameworkCore;
using Pulse.Posts.Domain;

namespace Pulse.Posts.Data;

internal class PostsContext(DbContextOptions<PostsContext> options) : DbContext(options)
{
    public DbSet<Post> PostSet { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new PostsMap());
        modelBuilder.ApplyConfiguration(new AttachmentMetadataMap());
    }
}
