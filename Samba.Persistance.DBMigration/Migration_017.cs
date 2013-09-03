using System.Data.Entity;
using FluentMigrator;

namespace Samba.Persistance.DBMigration
{
    [Migration(17)]
    public class Migration_017 : Migration
    {
        public override void Up()
        {
            Create.Column("Warehouse").OnTable("InventoryItems").AsString(128).Nullable();
        }

        public override void Down()
        {

        }
    }
}