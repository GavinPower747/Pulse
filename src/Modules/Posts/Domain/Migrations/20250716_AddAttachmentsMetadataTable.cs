using FluentMigrator;

namespace Pulse.Posts.Migrations;

[Migration(20250716)]
public class AddAttachmentsMetadataTable : Migration
{
    public override void Up()
    {
        Create.Table("attachment_metadata")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("post_id").AsGuid().NotNullable().ForeignKey("posts", "id")
            .WithColumn("type").AsInt32().NotNullable()
            .WithColumn("size").AsInt64().NotNullable()
            .WithColumn("content_type").AsString().NotNullable();

        Create.Index("ix_attachment_metadata_post_id").OnTable("attachment_metadata").OnColumn("post_id");
    }

    public override void Down()
    {
        Delete.Table("attachment_metadata");
    }
}