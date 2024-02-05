using FluentMigrator;

namespace Pulse.Posts.Migrations;

[Migration(20240128)]
public class AddPostsTable : Migration
{
    public override void Up()
    {
        Create.Table("posts")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("user_id").AsGuid()
            .WithColumn("content").AsString()
            .WithColumn("created_at").AsDateTime()
            .WithColumn("scheduled_at").AsDateTime().Nullable()
            .WithColumn("published_at").AsDateTime().Nullable();

        Create.Index("ix_posts_user_id").OnTable("posts").OnColumn("user_id");
    }

    public override void Down()
    {
        Delete.Table("Posts");
    }
}