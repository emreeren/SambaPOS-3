using System.Data.Entity;
using FluentMigrator;

namespace Samba.Persistance.DBMigration
{
    [Migration(18)]
    public class Migration_018 : Migration
    {
        public override void Up()
        {
            var dc = ApplicationContext as DbContext;
            Create.Column("AutomationCommandMapData").OnTable("AccountScreens").AsString(int.MaxValue).Nullable();
        }

        public override void Down()
        {

        }
    }
}