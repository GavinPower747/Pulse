using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pulse.Posts.Domain;

namespace Pulse.Posts.Data;

internal class PostsMap : IEntityTypeConfiguration<Post>
{
    public void Configure(EntityTypeBuilder<Post> builder)
    {
        builder.ToTable("posts");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id).HasColumnName("id").ValueGeneratedOnAdd();
        builder.Property(p => p.Content).HasColumnName("content").IsRequired();
        builder.Property(p => p.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(p => p.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(p => p.UpdatedAt).HasColumnName("updated_at").IsRequired(false);
        builder.Property(p => p.PublishedAt).HasColumnName("published_at").IsRequired(false);

        builder
            .HasMany(p => p.Attachments)
            .WithOne()
            .HasForeignKey(a => a.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(p => p.UserId);
    }
}
