using FluentMigrator;

namespace LMLZ.BootstrapNode.Migrations;

[Migration(1, "Initialize bootstrap node schema")]
public class Initialize : Migration
{
    public override void Up()
    {
        Create.Table("Peers")
            .WithColumn("Id").AsGuid().PrimaryKey().NotNullable().WithDefaultValue("NEWID()")
            .WithColumn("IP").AsString(45).NotNullable()
            .WithColumn("Port").AsInt32().NotNullable();
    }

    public override void Down()
    {
        Delete.Table("Peers");
    }
}