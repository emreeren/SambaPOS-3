using FluentMigrator;

namespace Samba.Persistance.DBMigration
{
    [Migration(8)]
    public class Migration_008 : Migration
    {
        public override void Up()
        {
            Create.Column("HideZeroBalanceAccounts").OnTable("AccountScreenValues").AsBoolean().WithDefaultValue(false);
            Create.Column("DisplayAsTree").OnTable("AccountScreens").AsBoolean().WithDefaultValue(false);
        }

        public override void Down()
        {

        }
    }
}