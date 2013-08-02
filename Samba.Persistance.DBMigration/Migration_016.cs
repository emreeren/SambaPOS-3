using System.Data.Entity;
using FluentMigrator;

namespace Samba.Persistance.DBMigration
{
    [Migration(16)]
    public class Migration_016 : Migration
    {
        public override void Up()
        {
            var dc = ApplicationContext as DbContext;
            Create.Index("IX_EntityStateValue_EntityId").OnTable("EntityStateValues").OnColumn("EntityId").Unique();
        }

        public override void Down()
        {

        }
    }
}