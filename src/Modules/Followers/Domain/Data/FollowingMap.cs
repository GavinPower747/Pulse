using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pulse.Followers.Domain;

namespace Pulse.Followers.Data;

internal class FollowingMap : IEntityTypeConfiguration<Following>
{
    public void Configure(EntityTypeBuilder<Following> builder)
    {
        builder.ToTable("followings");
        builder.HasKey(x => new { x.UserId, x.FollowingId });

        builder.Property(x => x.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(x => x.FollowingId).HasColumnName("following_id").IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();

        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.FollowingId);
    }
}
