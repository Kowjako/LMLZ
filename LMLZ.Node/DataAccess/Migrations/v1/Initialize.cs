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

        Create.Table("Peers")
              .WithColumn("Id").AsInt32().PrimaryKey().Identity()
              .WithColumn("IP").AsString(100).NotNullable()
              .WithColumn("Port").AsInt32().NotNullable();

        Create.Index("IX__Peers_Ip_Port")
              .OnTable("Peers")
              .OnColumn("IP").Ascending()
              .OnColumn("Port").Ascending()
              .WithOptions().Unique();

        Create.Table("Blocks")
            .WithColumn("Index").AsInt64().PrimaryKey() // No Identity here, blockchain maintains order
            .WithColumn("Hash").AsString().NotNullable()
            .WithColumn("Transactions").AsString().NotNullable()
            .WithColumn("Timestamp").AsDateTime().NotNullable()
            .WithColumn("Nonce").AsInt32().NotNullable()
            .WithColumn("MerkleRoot").AsString().NotNullable()
            .WithColumn("PreviousHash").AsString().NotNullable()
            .WithColumn("BlockSize").AsInt32().NotNullable()
            .WithColumn("Version").AsString().NotNullable();
    }

    public override void Down()
    {
        Delete.Table("Blocks");
        Delete.Index("IX__Peers_Ip_Port");
        Delete.Table("Peers");
        Delete.Table("Wallets");
    }
}
