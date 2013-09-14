using System.Globalization;
using System.IO;
using System.Linq;
using Samba.Domain.Models.Automation;
using Samba.Domain.Models.Settings;
using Samba.Infrastructure.Data;
using Samba.Infrastructure.Settings;
using Samba.Localization.Properties;
using Samba.Services.Common;

namespace Samba.Presentation.Services.Common.DataGeneration
{
    public class RuleGenerator
    {
        private ParameterBuilder Params()
        {
            return new ParameterBuilder();
        }

        private string GetResource(string resourceName, int type)
        {
            if (type == 1) return Resources.ResourceManager.GetString(resourceName, CultureInfo.InstalledUICulture);
            if (type == 2) return Resources.ResourceManager.GetString(resourceName, CultureInfo.InvariantCulture);
            return Resources.ResourceManager.GetString(resourceName);
        }

        private string GetResourceF(string formatResource, string resource, int type)
        {
            return string.Format(GetResource(formatResource, type), GetResource(resource, type));
        }

        private void DeleteEntity<T>(IWorkspace workspace, params string[] commandName) where T : class, IEntityClass
        {
            var name = GetName(commandName, 0);
            var acs = workspace.All<T>(x => x.Name == name).ToList();
            if (acs.Any())
            {
                foreach (var ac in acs)
                {
                    workspace.Delete(ac);
                    workspace.CommitChanges();
                }
            }

            var name2 = GetName(commandName, 1);
            acs = workspace.All<T>(x => x.Name == name2).ToList();
            if (acs.Any())
            {
                foreach (var ac in acs)
                {
                    workspace.Delete(ac);
                    workspace.CommitChanges();
                }
            }

            var name3 = GetName(commandName, 2);
            acs = workspace.All<T>(x => x.Name == name3).ToList();
            if (acs.Any())
            {
                foreach (var ac in acs)
                {
                    workspace.Delete(ac);
                    workspace.CommitChanges();
                }
            }
        }

        private string GetName(string[] commandName, int type)
        {
            var result = "";
            if (commandName.Count() == 3)
                result = string.Format(GetResource(commandName[0], type), GetResourceF(commandName[1], commandName[2], type), type);
            if (commandName.Count() == 2)
                result = GetResourceF(commandName[0], commandName[1], type);
            if (commandName.Count() == 1)
                result = GetResource(commandName[0], type);
            return result;
        }

        public static bool ShouldRegenerateRules()
        {
            return File.Exists(LocalSettings.UserPath + "\\regen.txt");
        }

        public void RegenerateRules(IWorkspace workspace)
        {
            if (!ShouldRegenerateRules()) return;

            DeleteEntities(workspace);
            GenerateSystemRules(workspace);
            File.Delete(LocalSettings.UserPath + "\\regen.txt");
        }

        private void DeleteEntities(IWorkspace workspace)
        {
            DeleteEntity<AutomationCommand>(workspace, "PrintBill");
            DeleteEntity<AutomationCommand>(workspace, "UnlockTicket");
            DeleteEntity<AutomationCommand>(workspace, "Add_f", "Ticket");
            DeleteEntity<AutomationCommand>(workspace, "Gift");
            DeleteEntity<AutomationCommand>(workspace, "Cancel_f", "Gift");
            DeleteEntity<AutomationCommand>(workspace, "Void");
            DeleteEntity<AutomationCommand>(workspace, "Cancel_f", "Void");
            DeleteEntity<AutomationCommand>(workspace, "CloseTicket");
            DeleteEntity<AutomationCommand>(workspace, "Settle");

            DeleteEntity<State>(workspace, "NewOrders");
            DeleteEntity<State>(workspace, "Available");
            DeleteEntity<State>(workspace, "BillRequested");

            DeleteEntity<AppAction>(workspace, "Update_f", "Order");
            DeleteEntity<AppAction>(workspace, "UpdateTicketStatus");
            DeleteEntity<AppAction>(workspace, "Update_f", "OrderStatus");
            DeleteEntity<AppAction>(workspace, "UpdateOrderGiftState");
            DeleteEntity<AppAction>(workspace, "UpdateEntityState");
            DeleteEntity<AppAction>(workspace, "Create_f", "Ticket");
            DeleteEntity<AppAction>(workspace, "CloseTicket");
            DeleteEntity<AppAction>(workspace, "DisplayPaymentScreen");
            DeleteEntity<AppAction>(workspace, "ExecutePrintBillJob");
            DeleteEntity<AppAction>(workspace, "ExecuteKitchenOrdersPrintJob");
            DeleteEntity<AppAction>(workspace, "LockTicket");
            DeleteEntity<AppAction>(workspace, "UnlockTicket");
            DeleteEntity<AppAction>(workspace, "MarkTicketAsClosed");

            DeleteEntity<AppRule>(workspace, "NewTicketCreatingRule");
            DeleteEntity<AppRule>(workspace, "NewOrderAddingRule");
            DeleteEntity<AppRule>(workspace, "TicketPaymentCheck");
            DeleteEntity<AppRule>(workspace, "TicketMovedRule");
            DeleteEntity<AppRule>(workspace, "Rule_f", "TicketClosing");
            DeleteEntity<AppRule>(workspace, "Rule_f", "Gift");
            DeleteEntity<AppRule>(workspace, "Rule_f", "Cancel_f", "Gift");
            DeleteEntity<AppRule>(workspace, "Rule_f", "Void");
            DeleteEntity<AppRule>(workspace, "Rule_f", "Cancel_f", "Void");

            DeleteEntity<AppRule>(workspace, "UpdateNewOrderEntityColor");
            DeleteEntity<AppRule>(workspace, "UpdateAvailableEntityColor");
            DeleteEntity<AppRule>(workspace, "UpdateMovedEntityColor");

            DeleteEntity<AppRule>(workspace, "Rule_f", "PrintBill");
            DeleteEntity<AppRule>(workspace, "Rule_f", "UnlockTicket");
            DeleteEntity<AppRule>(workspace, "Rule_f", "Create_f", "Ticket");
            DeleteEntity<AppRule>(workspace, "UpdateMergedTicketsState");

            DeleteEntity<AppRule>(workspace, "Rule_f", "CloseTicket");
            DeleteEntity<AppRule>(workspace, "Rule_f", "Settle");
        }

        public void GenerateSystemRules(IWorkspace workspace)
        {
            var closeTicketAutomation = new AutomationCommand { Name = Resources.CloseTicket, ButtonHeader = Resources.Close, SortOrder = -1, Color = "#FFFF0000", FontSize = 40 };
            closeTicketAutomation.AutomationCommandMaps.Add(new AutomationCommandMap { EnabledStates = string.Format("{0},{1},{2},{3},IsClosed", Resources.New, Resources.NewOrders, Resources.Unpaid, Resources.Locked), VisibleStates = "*", DisplayUnderTicket = true });
            workspace.Add(closeTicketAutomation);

            var settleAutomation = new AutomationCommand { Name = Resources.Settle, ButtonHeader = Resources.Settle, SortOrder = -2, FontSize = 40 };
            settleAutomation.AutomationCommandMaps.Add(new AutomationCommandMap { EnabledStates = "*", VisibleStates = "*", DisplayUnderTicket = true });
            workspace.Add(settleAutomation);

            var printBillAutomation = new AutomationCommand { Name = Resources.PrintBill, ButtonHeader = Resources.PrintBill, SortOrder = -1 };
            printBillAutomation.AutomationCommandMaps.Add(new AutomationCommandMap { EnabledStates = Resources.NewOrders + "," + Resources.Unpaid + ",IsClosed", VisibleStates = "*", DisplayOnTicket = true, DisplayOnPayment = true });
            workspace.Add(printBillAutomation);

            var unlockTicketAutomation = new AutomationCommand { Name = Resources.UnlockTicket, ButtonHeader = Resources.UnlockTicket, SortOrder = -1 };
            unlockTicketAutomation.AutomationCommandMaps.Add(new AutomationCommandMap { EnabledStates = Resources.Locked, VisibleStates = Resources.Locked, DisplayOnTicket = true });
            workspace.Add(unlockTicketAutomation);

            var addTicketAutomation = new AutomationCommand { Name = string.Format(Resources.Add_f, Resources.Ticket), ButtonHeader = string.Format(Resources.Add_f, Resources.Ticket), SortOrder = -1 };
            addTicketAutomation.AutomationCommandMaps.Add(new AutomationCommandMap { EnabledStates = string.Format("{0},{1}", Resources.Unpaid, Resources.Locked), VisibleStates = "*", DisplayOnTicket = true });
            workspace.Add(addTicketAutomation);

            var giftItemAutomation = new AutomationCommand { Name = Resources.Gift, ButtonHeader = Resources.Gift, SortOrder = -1 };
            giftItemAutomation.AutomationCommandMaps.Add(new AutomationCommandMap { EnabledStates = "GStatus=", VisibleStates = "GStatus=", DisplayOnOrders = true });
            workspace.Add(giftItemAutomation);

            var cancelGiftItemAutomation = new AutomationCommand { Name = string.Format(Resources.Cancel_f, Resources.Gift), ButtonHeader = string.Format(Resources.Cancel_f, Resources.Gift), SortOrder = -1 };
            cancelGiftItemAutomation.AutomationCommandMaps.Add(new AutomationCommandMap { EnabledStates = Resources.Gift, VisibleStates = Resources.Gift, DisplayOnOrders = true });
            workspace.Add(cancelGiftItemAutomation);

            var voidItemAutomation = new AutomationCommand { Name = Resources.Void, ButtonHeader = Resources.Void, SortOrder = -1 };
            voidItemAutomation.AutomationCommandMaps.Add(new AutomationCommandMap { EnabledStates = string.Format("GStatus={0}," + Resources.Submitted, Resources.Gift), VisibleStates = string.Format("GStatus=,GStatus={0}", Resources.Gift), DisplayOnOrders = true });
            workspace.Add(voidItemAutomation);

            var cancelVoidItemAutomation = new AutomationCommand { Name = string.Format(Resources.Cancel_f, Resources.Void), ButtonHeader = string.Format(Resources.Cancel_f, Resources.Void), SortOrder = -1 };
            cancelVoidItemAutomation.AutomationCommandMaps.Add(new AutomationCommandMap { EnabledStates = Resources.New, VisibleStates = Resources.Void, DisplayOnOrders = true });
            workspace.Add(cancelVoidItemAutomation);

            var newOrderState = new State { Name = Resources.NewOrders, Color = "Orange", GroupName = "Status" };
            workspace.Add(newOrderState);

            var availableState = new State { Name = Resources.Available, Color = "White", GroupName = "Status" };
            workspace.Add(availableState);

            var billRequestedState = new State { Name = Resources.BillRequested, Color = "Maroon", GroupName = "Status" };
            workspace.Add(billRequestedState);

            var giftStatus = new State { Name = Resources.Gift, GroupName = "GStatus", ShowOnEndOfDayReport = true, ShowOnProductReport = true, ShowOnTicket = true };
            workspace.Add(giftStatus);

            var status = new State { Name = Resources.Status, GroupName = "Status", ShowOnEndOfDayReport = true, ShowOnProductReport = false, ShowOnTicket = true };
            workspace.Add(status);

            var updateOrderAction = new AppAction
                                        {
                                            ActionType = ActionNames.UpdateOrder,
                                            Name = string.Format(Resources.Update_f, Resources.Order),
                                            Parameter = Params()
                                                .Add("IncreaseInventory", "[:Increase]").Add("DecreaseInventory", "[:Decrease]")
                                                .Add("CalculatePrice", "[:Calculate Price]").Add("Locked", "[:Locked]").
                                                 ToString(),
                                            SortOrder = -1
                                        };
            workspace.Add(updateOrderAction);

            var updateTicketStatusAction = new AppAction { ActionType = ActionNames.UpdateTicketState, Name = Resources.UpdateTicketStatus, Parameter = Params().Add("StateName", Resources.Status).Add("State", "[:Status]").Add("CurrentState", "[:Current Status]").ToString(), SortOrder = -1 };
            workspace.Add(updateTicketStatusAction);
            var updateOrderStatusAction = new AppAction { ActionType = ActionNames.UpdateOrderState, Name = string.Format(Resources.Update_f, Resources.OrderStatus), Parameter = Params().Add("StateName", "Status").Add("State", "[:Status]").Add("CurrentState", "[:Current Status]").ToString(), SortOrder = -1 };
            workspace.Add(updateOrderStatusAction);
            var updateOrderGiftStatusAction = new AppAction
                                                  {
                                                      ActionType = ActionNames.UpdateOrderState,
                                                      Name = Resources.UpdateOrderGiftState,
                                                      Parameter = Params()
                                                          .Add("StateName", "GStatus").Add("GroupOrder", "1")
                                                          .Add("CurrentState", "[:Current Status]").Add("State", "[:Status]")
                                                          .Add("StateOrder", "1").Add("StateValue", "[:Value]")
                                                          .ToString(),
                                                      SortOrder = -1
                                                  };
            workspace.Add(updateOrderGiftStatusAction);

            var updateEntityStateAction = new AppAction { ActionType = ActionNames.UpdateEntityState, Name = Resources.UpdateEntityState, Parameter = Params().Add("EntityStateName", "Status").Add("EntityState", "[:Status]").ToString(), SortOrder = -1 };
            workspace.Add(updateEntityStateAction);
            var createTicketAction = new AppAction { ActionType = ActionNames.CreateTicket, Name = string.Format(Resources.Create_f, Resources.Ticket), Parameter = "", SortOrder = -1 };
            workspace.Add(createTicketAction);
            var closeTicketAction = new AppAction { ActionType = ActionNames.CloseActiveTicket, Name = Resources.CloseTicket, Parameter = "", SortOrder = -1 };
            workspace.Add(closeTicketAction);
            var displayPaymentScreenAction = new AppAction { ActionType = ActionNames.DisplayPaymentScreen, Name = Resources.DisplayPaymentScreen, Parameter = "", SortOrder = -1 };
            workspace.Add(displayPaymentScreenAction);
            var printBillAction = new AppAction { ActionType = ActionNames.ExecutePrintJob, Name = Resources.ExecutePrintBillJob, Parameter = Params().Add("PrintJobName", Resources.PrintBill).ToString(), SortOrder = -1 };
            workspace.Add(printBillAction);
            var printKitchenOrdersAction = new AppAction { ActionType = ActionNames.ExecutePrintJob, Name = Resources.ExecuteKitchenOrdersPrintJob, Parameter = Params().Add("PrintJobName", Resources.PrintOrdersToKitchenPrinter).Add("OrderStateName", "Status").Add("OrderState", Resources.New).ToString(), SortOrder = -1 };
            workspace.Add(printKitchenOrdersAction);
            var lockTicketAction = new AppAction { ActionType = ActionNames.LockTicket, Name = Resources.LockTicket, Parameter = "", SortOrder = -1 };
            workspace.Add(lockTicketAction);
            var unlockTicketAction = new AppAction { ActionType = ActionNames.UnlockTicket, Name = Resources.UnlockTicket, Parameter = "", SortOrder = -1 };
            workspace.Add(unlockTicketAction);
            var markTicketAsClosed = new AppAction { ActionType = ActionNames.MarkTicketAsClosed, Name = Resources.MarkTicketAsClosed, Parameter = "", SortOrder = -1 };
            workspace.Add(markTicketAsClosed);
            workspace.CommitChanges();

            var newTicketRule = new AppRule { Name = Resources.NewTicketCreatingRule, EventName = RuleEventNames.TicketCreated, SortOrder = -1 };
            newTicketRule.Actions.Add(new ActionContainer(updateTicketStatusAction) { ParameterValues = string.Format("Status={0}", Resources.New) });
            newTicketRule.AddRuleMap();
            workspace.Add(newTicketRule);

            var newOrderAddingRule = new AppRule { Name = Resources.NewOrderAddingRule, EventName = RuleEventNames.OrderAdded, SortOrder = -1 };
            newOrderAddingRule.Actions.Add(new ActionContainer(updateTicketStatusAction) { ParameterValues = string.Format("Status={0}", Resources.NewOrders) });
            newOrderAddingRule.Actions.Add(new ActionContainer(updateOrderStatusAction) { ParameterValues = string.Format("Status={0}", Resources.New) });
            newOrderAddingRule.AddRuleMap();
            workspace.Add(newOrderAddingRule);

            var ticketPayCheckRule = new AppRule { Name = Resources.TicketPaymentCheck, EventName = RuleEventNames.BeforeTicketClosing, EventConstraints = "RemainingAmount;=;0", SortOrder = -1 };
            ticketPayCheckRule.Actions.Add(new ActionContainer(updateTicketStatusAction) { ParameterValues = "Status=" + Resources.Paid });
            ticketPayCheckRule.Actions.Add(new ActionContainer(markTicketAsClosed));
            ticketPayCheckRule.AddRuleMap();
            workspace.Add(ticketPayCheckRule);

            var ticketMovedRule = new AppRule { Name = Resources.TicketMovedRule, EventName = RuleEventNames.TicketMoved, SortOrder = -1 };
            ticketMovedRule.Actions.Add(new ActionContainer(updateTicketStatusAction) { ParameterValues = string.Format("Status={0}", Resources.NewOrders) });
            ticketMovedRule.AddRuleMap();
            workspace.Add(ticketMovedRule);

            var ticketClosingRule = new AppRule { Name = string.Format(Resources.Rule_f, Resources.TicketClosing), EventName = RuleEventNames.TicketClosing, SortOrder = -1 };
            ticketClosingRule.Actions.Add(new ActionContainer(printKitchenOrdersAction));
            ticketClosingRule.Actions.Add(new ActionContainer(updateTicketStatusAction) { ParameterValues = string.Format("Status={0}#Current Status={1}", Resources.Unpaid, Resources.NewOrders) });
            ticketClosingRule.Actions.Add(new ActionContainer(updateOrderStatusAction) { ParameterValues = string.Format("Status={0}#Current Status={1}", Resources.Submitted, Resources.New) });
            ticketClosingRule.AddRuleMap();
            workspace.Add(ticketClosingRule);

            var giftOrderRule = new AppRule { Name = string.Format(Resources.Rule_f, Resources.Gift), EventName = RuleEventNames.AutomationCommandExecuted, EventConstraints = string.Format("AutomationCommandName;=;{0}", giftItemAutomation.Name), SortOrder = -1 };
            giftOrderRule.Actions.Add(new ActionContainer(updateOrderAction) { ParameterValues = "Decrease=True#Calculate Price=False" });
            giftOrderRule.Actions.Add(new ActionContainer(updateOrderGiftStatusAction) { ParameterValues = string.Format("Status={0}#Value=[:Value]", Resources.Gift) });
            giftOrderRule.AddRuleMap();
            workspace.Add(giftOrderRule);

            var cancelGiftOrderRule = new AppRule { Name = string.Format(Resources.Rule_f, string.Format(Resources.Cancel_f, Resources.Gift)), EventName = RuleEventNames.AutomationCommandExecuted, EventConstraints = string.Format("AutomationCommandName;=;{0}", cancelGiftItemAutomation.Name), SortOrder = -1 };
            cancelGiftOrderRule.Actions.Add(new ActionContainer(updateOrderAction) { ParameterValues = "Decrease=True#Calculate Price=True" });
            cancelGiftOrderRule.Actions.Add(new ActionContainer(updateOrderGiftStatusAction) { ParameterValues = string.Format("Current Status={0}#Status=#Value=", Resources.Gift) });
            cancelGiftOrderRule.AddRuleMap();
            workspace.Add(cancelGiftOrderRule);

            var voidOrderRule = new AppRule { Name = string.Format(Resources.Rule_f, Resources.Void), EventName = RuleEventNames.AutomationCommandExecuted, EventConstraints = string.Format("AutomationCommandName;=;{0}", voidItemAutomation.Name), SortOrder = -1 };
            voidOrderRule.Actions.Add(new ActionContainer(updateOrderAction) { ParameterValues = "Decrease=False#Calculate Price=False" });
            voidOrderRule.Actions.Add(new ActionContainer(updateOrderGiftStatusAction) { ParameterValues = string.Format("Status={0}#Value=[:Value]", Resources.Void) });
            voidOrderRule.Actions.Add(new ActionContainer(updateOrderStatusAction) { ParameterValues = string.Format("Status={0}", Resources.New) });
            voidOrderRule.AddRuleMap();
            workspace.Add(voidOrderRule);

            var cancelVoidOrderRule = new AppRule { Name = string.Format(Resources.Rule_f, string.Format(Resources.Cancel_f, Resources.Void)), EventName = RuleEventNames.AutomationCommandExecuted, EventConstraints = string.Format("AutomationCommandName;=;{0}", cancelVoidItemAutomation.Name), SortOrder = -1 };
            cancelVoidOrderRule.Actions.Add(new ActionContainer(updateOrderAction) { ParameterValues = "Decrease=True#Calculate Price=True" });
            cancelVoidOrderRule.Actions.Add(new ActionContainer(updateOrderGiftStatusAction) { ParameterValues = string.Format("Current Status={0}#Status=#Value=", Resources.Void) });
            cancelVoidOrderRule.Actions.Add(new ActionContainer(updateOrderStatusAction) { ParameterValues = string.Format("Status={0}", Resources.Submitted) });
            cancelVoidOrderRule.AddRuleMap();
            workspace.Add(cancelVoidOrderRule);

            var newOrderRule = new AppRule { Name = Resources.UpdateNewOrderEntityColor, EventName = RuleEventNames.TicketStateUpdated, EventConstraints = "State;=;" + Resources.Unpaid, SortOrder = -1 };
            newOrderRule.Actions.Add(new ActionContainer(updateEntityStateAction) { ParameterValues = "Status=" + Resources.NewOrders });
            newOrderRule.AddRuleMap();
            workspace.Add(newOrderRule);

            var availableRule = new AppRule { Name = Resources.UpdateAvailableEntityColor, EventName = RuleEventNames.EntityUpdated, EventConstraints = "OpenTicketCount;=;0", SortOrder = -1 };
            var ac2 = new ActionContainer(updateEntityStateAction) { ParameterValues = string.Format("Status={0}", Resources.Available) };
            availableRule.Actions.Add(ac2);
            availableRule.AddRuleMap();
            workspace.Add(availableRule);

            var movingRule = new AppRule { Name = Resources.UpdateMovedEntityColor, EventName = "TicketEntityChanged", EventConstraints = "OrderCount;>;0", SortOrder = -1 };
            var ac3 = new ActionContainer(updateEntityStateAction) { ParameterValues = string.Format("Status={0}", Resources.NewOrders) };
            movingRule.Actions.Add(ac3);
            movingRule.AddRuleMap();
            workspace.Add(movingRule);

            var printBillRule = new AppRule { Name = string.Format(Resources.Rule_f, Resources.PrintBill), EventName = RuleEventNames.AutomationCommandExecuted, EventConstraints = "AutomationCommandName;=;" + Resources.PrintBill, SortOrder = -1 };
            printBillRule.Actions.Add(new ActionContainer(printBillAction));
            printBillRule.Actions.Add(new ActionContainer(lockTicketAction));
            printBillRule.Actions.Add(new ActionContainer(updateEntityStateAction) { ParameterValues = string.Format("Status={0}", Resources.BillRequested) });
            printBillRule.Actions.Add(new ActionContainer(updateTicketStatusAction) { ParameterValues = string.Format("Status={0}", Resources.Locked) });
            printBillRule.Actions.Add(new ActionContainer(closeTicketAction));
            printBillRule.AddRuleMap();
            workspace.Add(printBillRule);

            var unlockTicketRule = new AppRule { Name = string.Format(Resources.Rule_f, Resources.UnlockTicket), EventName = RuleEventNames.AutomationCommandExecuted, EventConstraints = "AutomationCommandName;=;" + Resources.UnlockTicket, SortOrder = -1 };
            unlockTicketRule.Actions.Add(new ActionContainer(unlockTicketAction));
            unlockTicketRule.Actions.Add(new ActionContainer(updateTicketStatusAction) { ParameterValues = string.Format("Status={0}", Resources.Unpaid) });
            unlockTicketRule.AddRuleMap();
            workspace.Add(unlockTicketRule);

            var createTicketRule = new AppRule { Name = string.Format(Resources.Rule_f, string.Format(Resources.Create_f, Resources.Ticket)), EventName = RuleEventNames.AutomationCommandExecuted, EventConstraints = "AutomationCommandName;=;" + string.Format(Resources.Add_f, Resources.Ticket), SortOrder = -1 };
            createTicketRule.Actions.Add(new ActionContainer(createTicketAction));
            createTicketRule.AddRuleMap();
            workspace.Add(createTicketRule);

            var updateMergedTicket = new AppRule { Name = Resources.UpdateMergedTicketsState, EventName = RuleEventNames.TicketsMerged, SortOrder = -1 };
            updateMergedTicket.Actions.Add(new ActionContainer(updateEntityStateAction) { ParameterValues = string.Format("Status={0}", Resources.NewOrders) });
            updateMergedTicket.Actions.Add(new ActionContainer(updateTicketStatusAction) { ParameterValues = string.Format("Status={0}", Resources.NewOrders) });
            updateMergedTicket.AddRuleMap();
            workspace.Add(updateMergedTicket);

            var closeTicketRule = new AppRule { Name = string.Format(Resources.Rule_f, Resources.CloseTicket), EventName = RuleEventNames.AutomationCommandExecuted, EventConstraints = "AutomationCommandName;=;" + Resources.CloseTicket, SortOrder = -1 };
            closeTicketRule.Actions.Add(new ActionContainer(closeTicketAction));
            closeTicketRule.AddRuleMap();
            workspace.Add(closeTicketRule);

            var settleTicketRule = new AppRule { Name = string.Format(Resources.Rule_f, Resources.Settle), EventName = RuleEventNames.AutomationCommandExecuted, EventConstraints = "AutomationCommandName;=;" + Resources.Settle, SortOrder = -1 };
            settleTicketRule.Actions.Add(new ActionContainer(displayPaymentScreenAction));
            settleTicketRule.AddRuleMap();
            workspace.Add(settleTicketRule);

            workspace.CommitChanges();
        }
    }
}