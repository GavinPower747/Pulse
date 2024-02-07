using Microsoft.EntityFrameworkCore;
using Pulse.Posts.Domain;

namespace Pulse.Posts.Data;

internal class PostsMap : IEntityTypeConfiguration<Post>
{
    public void Configure(
        Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<Post> builder
    )
    {
        builder.ToTable("posts");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id).HasColumnName("id").ValueGeneratedOnAdd();
        builder.Property(p => p.Content).HasColumnName("content").IsRequired();
        builder.Property(p => p.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(p => p.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(p => p.UpdatedAt).HasColumnName("updated_at").IsRequired(false);
        builder.Property(p => p.PublishedAt).HasColumnName("published_at").IsRequired(false);
        builder.Property(p => p.ScheduledAt).HasColumnName("scheduled_at").IsRequired(false);
    }
}
