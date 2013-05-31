using System;
using System.Data.Entity;
using System.Linq;
using FluentMigrator;
using Samba.Domain.Models.Automation;
using Samba.Localization.Properties;

namespace Samba.Persistance.DBMigration
{
    [Migration(4)]
    public class Migration_004 : Migration
    {
        public override void Up()
        {
            Create.Column("TaxFree").OnTable("OrderTagGroups").AsBoolean().WithDefaultValue(false);
            Create.Column("Added").OnTable("PeriodicConsumptionItems").AsDecimal().WithDefaultValue(0);
            Create.Column("Removed").OnTable("PeriodicConsumptionItems").AsDecimal().WithDefaultValue(0);
            Execute.Sql("Update PeriodicConsumptionItems set Added = Purchase");
            Delete.Column("Purchase").FromTable("PeriodicConsumptionItems");

            var dc = ApplicationContext as DbContext;

            if (dc != null)
            {
                var ticketPayingRule = dc.Set<AppRule>().SingleOrDefault(x => x.Name == "Ticket Paying Rule");
                if (ticketPayingRule != null)
                {
                    ticketPayingRule.Name = "Ticket Payment Check";
                    ticketPayingRule.EventName = "BeforeTicketClosing";
                    var markTicketAsClosed = new AppAction { ActionType = "MarkTicketAsClosed", Name = Resources.MarkTicketAsClosed, Parameter = "" };
                    dc.Set<AppAction>().Add(markTicketAsClosed);
                    dc.SaveChanges();
                    ticketPayingRule.Actions.Add(new ActionContainer(markTicketAsClosed));
                }
                dc.SaveChanges();
            }
        }

        public override void Down()
        {

        }
    }
}