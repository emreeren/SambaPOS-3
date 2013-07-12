using FluentMigrator;

namespace Samba.Persistance.DBMigration
{
    [Migration(12)]
    public class Migration_012 : Migration
    {
        public override void Up()
        {
            Create.Column("OrderTags").OnTable("ScreenMenuItems").AsString(128).Nullable();
            Create.Column("OrderStates").OnTable("ScreenMenuItems").AsString(128).Nullable();
            Create.Column("AutomationCommand").OnTable("ScreenMenuItems").AsString(128).Nullable();
            Create.Column("AutomationCommandValue").OnTable("ScreenMenuItems").AsString(128).Nullable();
            Create.Column("Hidden").OnTable("OrderTagGroups").AsBoolean().WithDefaultValue(false);
        }

        public override void Down()
        {

        }
    }
}