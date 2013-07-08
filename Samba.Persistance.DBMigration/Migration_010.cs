using FluentMigrator;

namespace Samba.Persistance.DBMigration
{
    [Migration(10)]
    public class Migration_010 : Migration
    {
        public override void Up()
        {
            Create.Column("DisplayOnTicketList").OnTable("AutomationCommandMaps").AsBoolean().WithDefaultValue(false);
            Create.Column("Name").OnTable("Widgets").AsString(128).Nullable();
            Create.Column("CopyToNewTickets").OnTable("EntityTypeAssignments").AsBoolean().WithDefaultValue(false);
            Execute.Sql("Update EntityTypeAssignments set CopyToNewTickets=1");
        }

        public override void Down()
        {

        }
    }
}