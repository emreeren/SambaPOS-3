using System.Data.Entity;
using System.Linq;
using FluentMigrator;
using Samba.Domain.Models.Automation;
using Samba.Localization.Properties;
using Samba.Presentation.Services.Common;

namespace Samba.Persistance.DBMigration
{
    [Migration(14, TransactionBehavior.None)]
    public class Migration_014 : Migration
    {
        public override void Up()
        {
            var dc = ApplicationContext as DbContext;
            Create.Column("DisplayUnderTicket").OnTable("AutomationCommandMaps").AsBoolean().WithDefaultValue(false);
            Create.Column("FontSize").OnTable("OrderTagGroups").AsInt32().WithDefaultValue(0);
            Create.Column("ButtonColor").OnTable("OrderTagGroups").AsString(128).WithDefaultValue(0);
        }

        public override void Down()
        {

        }
    }
}