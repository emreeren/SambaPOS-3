using System;
using System.Data.Entity;
using System.Linq;
using FluentMigrator;
using Samba.Domain.Models.Automation;
using Samba.Localization.Properties;

namespace Samba.Persistance.DBMigration
{
    [Migration(2)]
    public class Migration_002 : Migration
    {
        public override void Up()
        {
            var dc = ApplicationContext as DbContext;

            if (dc != null)
            {
                var updateTicketStatusAction = dc.Set<AppAction>().SingleOrDefault(x => x.Name == Resources.UpdateTicketStatus);
                var updateMergedTicket = dc.Set<AppRule>().SingleOrDefault(x => x.Name == Resources.UpdateMergedTicketsState);
                if (updateTicketStatusAction != null)
                {
                    if (updateMergedTicket != null)
                    {
                        updateMergedTicket.Actions.Add(new ActionContainer(updateTicketStatusAction) { ParameterValues = string.Format("Status={0}", Resources.NewOrders) });
                    }

                    var ticketMovedRule = new AppRule { Name = Resources.TicketMovedRule, EventName = "TicketMoved" };
                    ticketMovedRule.Actions.Add(new ActionContainer(updateTicketStatusAction) { ParameterValues = string.Format("Status={0}", Resources.NewOrders) });
                    ticketMovedRule.AddRuleMap();
                    dc.Set<AppRule>().Add(ticketMovedRule);
                }
                dc.SaveChanges();
            }

            Create.Column("UsePlainSum").OnTable("Calculations").AsBoolean().WithDefaultValue(false);
            Create.Column("UsePlainSum").OnTable("CalculationTypes").AsBoolean().WithDefaultValue(false);
        }

        public override void Down()
        {

        }
    }
}