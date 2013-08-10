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
            Create.Column("IsRefundType").OnTable("TicketTypes").AsBoolean().WithDefaultValue(false);
            Create.Column("RefundsTicketId").OnTable("Tickets").AsInt32().WithDefaultValue(0);
        }

        public override void Down()
        {
            Delete.Column("IsRefundType").FromTable("TicketTypes");
            Delete.Column("RefundsTicketId").FromTable("Tickets");
        }
    }
}
