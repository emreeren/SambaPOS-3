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
            Create.Column("DisplayUnderTicket").OnTable("AutomationCommandMaps").AsBoolean().WithDefaultValue(false);
            Create.Column("FontSize").OnTable("OrderTagGroups").AsInt32().WithDefaultValue(0);
            Create.Column("ButtonColor").OnTable("OrderTagGroups").AsString(128).WithDefaultValue(0);
            Execute.Sql("Update OrderTagGroups set FontSize = 0 where FontSize is null");
        }

        public override void Down()
        {

        }
    }
}