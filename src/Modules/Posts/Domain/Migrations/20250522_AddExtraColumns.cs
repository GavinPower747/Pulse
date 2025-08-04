using FluentMigrator;

namespace Pulse.Posts.Migrations;

[Migration(20250522)]
public class AddExtraTimingColumns : Migration
{
    public override void Up()
    {
        Alter
            .Table("posts")
            .AddColumn("published_at")
            .AsDateTime()
            .Nullable()
            .AddColumn("updated_at")
            .AsDateTime()
            .Nullable();
    }

    public override void Down()
    {
        Delete.Column("published_at").FromTable("posts");
        Delete.Column("updated_at").FromTable("posts");
    }
}
