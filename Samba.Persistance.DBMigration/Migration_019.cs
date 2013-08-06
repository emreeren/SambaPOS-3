using System.Data.Entity;
using FluentMigrator;

namespace Samba.Persistance.DBMigration
{
    [Migration(19)]
    public class Migration_019 : Migration
    {
        public override void Up()
        {
            var dc = ApplicationContext as DbContext;
            Create.Column("IsVoidType").OnTable("TicketTypes").AsBoolean().WithDefaultValue(false);
            Create.Column("VoidsTicketId").OnTable("Tickets").AsInt32().Nullable();
        }

        public override void Down()
        {
            Delete.Column("IsVoidType").FromTable("TicketTypes");
            Delete.Column("VoidsTicketId").FromTable("Tickets");
        }
    }
}
