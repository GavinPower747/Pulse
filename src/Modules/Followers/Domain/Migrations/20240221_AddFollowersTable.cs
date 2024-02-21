using FluentMigrator;

namespace Pulse.Followers.Migrations;

[Migration(20240221)]
public class AddFollowersTable : Migration
{
    public override void Up()
    {
        Create
            .Table("followings")
            .WithColumn("id")
            .AsGuid()
            .PrimaryKey()
            .WithColumn("user_id")
            .AsGuid()
            .WithColumn("following_id")
            .AsGuid()
            .WithColumn("created_at")
            .AsDateTime();

        Create.Index("ix_followings_user_id").OnTable("followings").OnColumn("user_id");
        Create.Index("ix_followings_following_id").OnTable("followings").OnColumn("following_id");
    }

    public override void Down()
    {
        Delete.Table("followings");
    }
}
