using System.Data.Entity;
using System.Linq;
using FluentMigrator;
using Samba.Domain.Models.Automation;
using Samba.Localization.Properties;
using Samba.Presentation.Services.Common;

namespace Samba.Persistance.DBMigration
{
    [Migration(15)]
    public class Migration_015 : Migration
    {
        public override void Up()
        {
            var dc = ApplicationContext as DbContext;

            var closeTicketAutomation = new AutomationCommand { Name = Resources.CloseTicket, ButtonHeader = Resources.Close, SortOrder = -1, Color = "#FFFF0000",FontSize = 40};
            closeTicketAutomation.AutomationCommandMaps.Add(new AutomationCommandMap { EnabledStates = string.Format("{0},{1},{2},{3},IsClosed", Resources.New, Resources.NewOrders, Resources.Unpaid, Resources.Locked), VisibleStates = "*", DisplayUnderTicket = true });
            dc.Set<AutomationCommand>().Add(closeTicketAutomation);

            var settleAutomation = new AutomationCommand { Name = Resources.Settle, ButtonHeader = Resources.Settle, SortOrder = -2 ,FontSize = 40};
            settleAutomation.AutomationCommandMaps.Add(new AutomationCommandMap { EnabledStates = "*", VisibleStates = "*", DisplayUnderTicket = true });
            dc.Set<AutomationCommand>().Add(settleAutomation);

            dc.SaveChanges();

            var displayPaymentScreenAction = new AppAction { ActionType = ActionNames.DisplayPaymentScreen, Name = Resources.DisplayPaymentScreen, Parameter = "", SortOrder = -1 };
            dc.Set<AppAction>().Add(displayPaymentScreenAction);

            var closeTicketAction = dc.Set<AppAction>().FirstOrDefault(x => x.ActionType == ActionNames.CloseActiveTicket);

            if (closeTicketAction == null)
            {
                closeTicketAction = new AppAction { ActionType = ActionNames.CloseActiveTicket, Name = Resources.CloseTicket, Parameter = "", SortOrder = -1 };
                dc.Set<AppAction>().Add(closeTicketAction);
            }
            dc.SaveChanges();

            var closeTicketRule = new AppRule { Name = string.Format(Resources.Rule_f, Resources.CloseTicket), EventName = "AutomationCommandExecuted", EventConstraints = "AutomationCommandName;=;" + Resources.CloseTicket, SortOrder = -1 };
            closeTicketRule.Actions.Add(new ActionContainer(closeTicketAction));
            closeTicketRule.AddRuleMap();
            dc.Set<AppRule>().Add(closeTicketRule);

            var settleTicketRule = new AppRule { Name = string.Format(Resources.Rule_f, Resources.Settle), EventName = "AutomationCommandExecuted", EventConstraints = "AutomationCommandName;=;" + Resources.Settle, SortOrder = -1 };
            settleTicketRule.Actions.Add(new ActionContainer(displayPaymentScreenAction));
            settleTicketRule.AddRuleMap();
            dc.Set<AppRule>().Add(settleTicketRule);

            dc.SaveChanges();
        }

        public override void Down()
        {

        }
    }
}