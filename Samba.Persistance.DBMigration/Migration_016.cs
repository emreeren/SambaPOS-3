using System.Data.Entity;
using FluentMigrator;

namespace Samba.Persistance.DBMigration
{
    [Migration(16)]
    public class Migration_016 : Migration
    {
        public override void Up()
        {
            Create.Index("IX_EntityStateValue_EntityId").OnTable("EntityStateValues").OnColumn("EntityId").Unique();
            Delete.Column("ButtonColor").FromTable("OrderTagGroups");
            Create.Column("ButtonColor").OnTable("OrderTagGroups").AsString(128).Nullable();
        }

        public override void Down()
        {

        }
    }
}