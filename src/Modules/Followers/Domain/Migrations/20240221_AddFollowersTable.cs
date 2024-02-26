using FluentMigrator;

namespace Pulse.Followers.Migrations;

[Migration(20240221)]
public class AddFollowersTable : Migration
{
    public override void Up()
    {
        Create
            .Table("followings")
            .WithColumn("user_id")
            .AsGuid()
            .NotNullable()
            .WithColumn("following_id")
            .AsGuid()
            .NotNullable()
            .WithColumn("created_at")
            .AsDateTime();

        Create.PrimaryKey("pk_followings").OnTable("followings").Columns("user_id", "following_id");

        Create.Index("ix_followings_user_id").OnTable("followings").OnColumn("user_id");
        Create.Index("ix_followings_following_id").OnTable("followings").OnColumn("following_id");
    }

    public override void Down()
    {
        Delete.Table("followings");
    }
}
