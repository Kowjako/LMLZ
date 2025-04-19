using FluentMigrator;

namespace LMLZ.BootstrapNode.Migrations;

[Migration(1, "Initialize bootstrap node schema")]
public class Initialize : Migration
{
    public override void Up()
    {
        Create.Table("Peers")
            .WithColumn("Id").AsGuid().PrimaryKey().NotNullable().WithDefaultValue("lower(hex(randomblob(16)))")
            .WithColumn("IP").AsString(45).NotNullable()
            .WithColumn("Port").AsInt32().NotNullable()
            .WithColumn("LastSeen").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime);

        Create.Index("IX__Peers_Ip_Port")
              .OnTable("Peers")
              .OnColumn("IP").Ascending()
              .OnColumn("Port").Ascending()
              .WithOptions().Unique();
    }

    public override void Down()
    {
        Delete.Index("IX__Peers_Ip_Port");
        Delete.Table("Peers");
    }
}