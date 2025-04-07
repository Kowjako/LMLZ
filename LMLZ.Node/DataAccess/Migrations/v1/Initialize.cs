using FluentMigrator;

namespace LMLZ.Node.DataAccess.Migrations.v1;

[Migration(1, "Setup basic blockchain database")]
public class Initialize : Migration
{
    public override void Up()
    {
        Create.Table("Wallets")
              .WithColumn("Id").AsInt32().PrimaryKey().Identity()
              .WithColumn("Address").AsString(100).NotNullable()
              .WithColumn("Name").AsString(100).Unique().NotNullable()
              .WithColumn("PublicKey").AsString(100).NotNullable()
              .WithColumn("PrivateKeyProtected").AsString(100).NotNullable();
    }

    public override void Down()
    {
        Delete.Table("Wallets");
    }
}
