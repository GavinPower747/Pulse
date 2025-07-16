using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Pulse.Posts.Domain;

namespace Pulse.Posts.Data;

internal class AttachmentMetadataMap : IEntityTypeConfiguration<AttachmentMetadata>
{
    public void Configure(EntityTypeBuilder<AttachmentMetadata> builder)
    {
        builder.ToTable("attachment_metadata");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id).HasColumnName("id").ValueGeneratedOnAdd();
        builder.Property(a => a.PostId).HasColumnName("post_id").IsRequired();
        builder.Property(a => a.Type).HasColumnName("type").IsRequired();
        builder.Property(a => a.Size).HasColumnName("size").IsRequired();
        builder.Property(a => a.ContentType).HasColumnName("content_type").IsRequired();
    }
}