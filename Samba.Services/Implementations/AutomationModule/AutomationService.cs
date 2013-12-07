using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reflection;
using System.Windows.Media;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Automation;
using Samba.Domain.Models.Entities;
using Samba.Domain.Models.Menus;
using Samba.Domain.Models.Settings;
using Samba.Domain.Models.Tickets;
using Samba.Domain.Models.Users;
using Samba.Localization.Properties;
using Samba.Persistance.Data;
using Samba.Services.Common;

namespace Samba.Services.Implementations.AutomationModule
{
    [Export(typeof(IAutomationService))]
    class AutomationService : IAutomationService
    {
        private readonly IDeviceService _deviceService;
        private readonly RuleActionTypeRegistry _ruleActionTypeRegistry;

        [ImportingConstructor]
        public AutomationService(IDeviceService deviceService, RuleActionTypeRegistry ruleActionTypeRegistry)
        {
            _deviceService = deviceService;
            _ruleActionTypeRegistry = ruleActionTypeRegistry;
        }

        public void ProcessAction(string actionType, ActionData actionData)
        {
            _ruleActionTypeRegistry.ProcessAction(actionType, actionData);
        }

        public IEnumerable<RuleConstraint> CreateRuleConstraints(string eventConstraints)
        {
            return eventConstraints.Split('#')
                .Select(x => new RuleConstraint(x));
        }

        public void RegisterEvent(string eventKey, string eventName, object constraintObject = null)
        {
            _ruleActionTypeRegistry.RegisterEvent(eventKey, eventName, constraintObject);
        }

        public IEnumerable<RuleConstraint> GetEventConstraints(string eventName)
        {
            return _ruleActionTypeRegistry.GetEventConstraints(eventName);
        }

        public IEnumerable<RuleEvent> GetRuleEvents()
        {
            return _ruleActionTypeRegistry.RuleEvents.Values;
        }

        IEnumerable<string> IAutomationService.GetParameterNames(string eventName)
        {
            return _ruleActionTypeRegistry.GetParameterNames(eventName);
        }

        public IActionType GetActionType(string value)
        {
            return _ruleActionTypeRegistry.ActionTypes.First(x => x.ActionKey == value);
        }

        public IEnumerable<IActionType> GetActionTypes()
        {
            return _ruleActionTypeRegistry.ActionTypes;
        }

        public IEnumerable<ParameterValue> CreateParameterValues(IActionType actionProcessor)
        {
            if (actionProcessor.ParameterObject != null)
                return actionProcessor.ParameterObject.Select(x => new ParameterValue(x.Key, x.Value.GetType()));
            return new List<ParameterValue>();
        }

        public void RegisterParameterSource(string parameterName, Func<IEnumerable<string>> action)
        {
            ParameterSources.Add(parameterName, action);
        }

        public void Register()
        {
            RegisterRules();
            RegisterParameterSources();
        }

        public IDictionary<string, Type> GetCustomRuleConstraintNames(string eventName)
        {
            return _ruleActionTypeRegistry.GetCustomRuleConstraintNames(eventName);
        }

        private void RegisterRules()
        {
            RegisterEvent(RuleEventNames.ApplicationScreenChanged, Resources.ApplicationScreenChanged, new { PreviousScreen = "", CurrentScreen = "" });
            RegisterEvent(RuleEventNames.AutomationCommandExecuted, Resources.AutomationCommandExecuted, new { AutomationCommandName = "", CommandValue = "", NextCommandValue = "" });
            RegisterEvent(RuleEventNames.TriggerExecuted, Resources.TriggerExecuted, new { TriggerName = "" });
            RegisterEvent(RuleEventNames.UserLoggedIn, Resources.UserLogin, new { UserName = "", RoleName = "" });
            RegisterEvent(RuleEventNames.UserLoggedOut, Resources.UserLogout, new { UserName = "", RoleName = "" });
            RegisterEvent(RuleEventNames.WorkPeriodStarts, Resources.WorkPeriodStarted);
            RegisterEvent(RuleEventNames.BeforeWorkPeriodEnds, Resources.BeforeWorkPeriodEnds);
            RegisterEvent(RuleEventNames.WorkPeriodEnds, Resources.WorkPeriodEnded);
            RegisterEvent(RuleEventNames.TicketCreated, Resources.TicketCreated, new { TicketTypeName = "" });
            RegisterEvent(RuleEventNames.TicketMoving, Resources.Ticket_Moving);
            RegisterEvent(RuleEventNames.TicketMoved, Resources.TicketMoved, new { OldTicketNumber = "" });
            RegisterEvent(RuleEventNames.TicketOpened, Resources.TicketOpened, new { OrderCount = 0 });
            RegisterEvent(RuleEventNames.BeforeTicketClosing, Resources.BeforeTicketClosing, new { TicketId = 0, RemainingAmount = 0m, TotalAmount = 0m });
            RegisterEvent(RuleEventNames.TicketClosing, Resources.TicketClosing, new { TicketId = 0, RemainingAmount = 0m, TotalAmount = 0m });
            RegisterEvent(RuleEventNames.TicketsMerged, Resources.TicketsMerged, new { TicketNumbers = "" });
            RegisterEvent(RuleEventNames.TicketEntityChanged, Resources.TicketEntityChanged, new { EntityTypeName = "", OrderCount = 0, OldEntityName = "", NewEntityName = "", OldCustomData = "", CustomData = "" });
            RegisterEvent(RuleEventNames.TicketTagSelected, Resources.TicketTagSelected, new { TagName = "", TagValue = "", NumericValue = 0, TicketTag = "" });
            RegisterEvent(RuleEventNames.TicketStateUpdated, Resources.TicketStateUpdated, new { StateName = "", State = "", StateValue = "", Quantity = 0, TicketState = "" });
            RegisterEvent(RuleEventNames.TicketTotalChanged, Resources.TicketTotalChanged, new { PreviousTotal = 0m, TicketTotal = 0m, RemainingAmount = 0m, DiscountTotal = 0m, PaymentTotal = 0m });
            RegisterEvent(RuleEventNames.PaymentProcessed, Resources.PaymentProcessed, new { PaymentTypeName = "", TenderedAmount = 0m, ProcessedAmount = 0m, ChangeAmount = 0m, RemainingAmount = 0m, SelectedQuantity = 0m });
            RegisterEvent(RuleEventNames.ChangeAmountChanged, Resources.ChangeAmountUpdated, new { TicketAmount = 0, ChangeAmount = 0, TenderedAmount = 0 });
            RegisterEvent(RuleEventNames.OrderAdded, Resources.OrderAddedToTicket, new { MenuItemGroupCode = "", MenuItemTag = "", MenuItemName = "" });
            RegisterEvent(RuleEventNames.OrderMoving, Resources.OrderMoving, new { MenuItemName = "", Quantity = 0m });
            RegisterEvent(RuleEventNames.OrderMoved, Resources.OrderMoved, new { MenuItemName = "", Quantity = 0m, OldTicketNumber = "" });
            RegisterEvent(RuleEventNames.OrderCancelled, Resources.OrderCancelled, new { MenuItemName = "", Quantity = 0m });
            RegisterEvent(RuleEventNames.OrderTagged, Resources.OrderTagged, new { OrderTagName = "", OrderTagValue = "" });
            RegisterEvent(RuleEventNames.OrderUntagged, Resources.OrderUntagged, new { OrderTagName = "", OrderTagValue = "" });
            RegisterEvent(RuleEventNames.OrderStateUpdated, Resources.OrderStateUpdated, new { StateName = "", State = "", StateValue = "", PreviousState = "" });
            RegisterEvent(RuleEventNames.EntitySelected, Resources.EntitySelected, new { EntityTypeName = "", EntityName = "", EntityCustomData = "", IsTicketSelected = false });
            RegisterEvent(RuleEventNames.EntityUpdated, Resources.EntityUpdated, new { EntityTypeName = "", OpenTicketCount = 0 });
            RegisterEvent(RuleEventNames.EntityStateUpdated, Resources.EntityStateUpdated, new { EntityTypeName = "", StateName = "", State = "", Quantity = 0m });
            RegisterEvent(RuleEventNames.AccountTransactionDocumentCreated, Resources.AccountTransactionDocumentCreated, new { AccountTransactionDocumentName = "", DocumentId = 0 });
            RegisterEvent(RuleEventNames.AccountTransactionAddedToTicket, Resources.AccountTransactionAddedToTicket, new { TransactionTypeName = "", SourceAccountName = "", TargetAccountName = "", Amount = 0m, ExchangeRate = 0m });
            RegisterEvent(RuleEventNames.MessageReceived, Resources.MessageReceived, new { Command = "" });
            RegisterEvent(RuleEventNames.DeviceEventGenerated, Resources.DeviceEventGenerated, new { DeviceName = "", EventName = "", EventData = "" });
            RegisterEvent(RuleEventNames.ApplicationStarted, Resources.ApplicationStarted, new { Arguments = "" });
            RegisterEvent(RuleEventNames.ValueLooped, Resources.ValueLooped, new { Name = "", LoopValue = "" });
            RegisterEvent(RuleEventNames.NumberpadValueEntered, Resources.NumberpadValueEntered, new { NumberpadValue = "" });
            RegisterEvent(RuleEventNames.PopupClicked, "Popup Clicked", new { Name = "", Data = "" });

            //Breaking changes 
            //NumberpadValueEntered > Value > NumberpadValue
            //AutomationCommandExecuted > Value > CommandValue
            //
        }

        private void RegisterParameterSources()
        {
            RegisterParameterSource("UserName", () => Dao.Distinct<User>(x => x.Name));
            RegisterParameterSource("DepartmentName", () => Dao.Distinct<Department>(x => x.Name));
            RegisterParameterSource("TerminalName", () => Dao.Distinct<Terminal>(x => x.Name));
            RegisterParameterSource("DeviceName", () => _deviceService.GetDeviceNames());
            RegisterParameterSource("TriggerName", () => Dao.Select<Trigger, string>(yz => yz.Name, y => !string.IsNullOrEmpty(y.Expression)));
            RegisterParameterSource("MenuItemName", () => Dao.Distinct<MenuItem>(yz => yz.Name));
            RegisterParameterSource("PriceTag", () => Dao.Distinct<MenuItemPriceDefinition>(x => x.PriceTag));
            RegisterParameterSource("Color", () => typeof(Colors).GetProperties(BindingFlags.Public | BindingFlags.Static).Select(x => x.Name));
            RegisterParameterSource("TaxTemplate", () => Dao.Distinct<TaxTemplate>(x => x.Name));
            RegisterParameterSource("CalculationType", () => Dao.Distinct<CalculationType>(x => x.Name));
            RegisterParameterSource("TagName", () => Dao.Distinct<TicketTagGroup>(x => x.Name));
            RegisterParameterSource("OrderTagName", () => Dao.Distinct<OrderTagGroup>(x => x.Name));
            RegisterParameterSource("State", () => Dao.Distinct<State>(x => x.Name));
            RegisterParameterSource("EntityState", () => Dao.Distinct<State>(x => x.Name, x => x.StateType == 0));
            RegisterParameterSource("TicketState", () => Dao.Distinct<State>(x => x.Name, x => x.StateType == 1));
            RegisterParameterSource("OrderState", () => Dao.Distinct<State>(x => x.Name, x => x.StateType == 2));
            RegisterParameterSource("StateName", () => Dao.Distinct<State>(x => x.GroupName));
            RegisterParameterSource("EntityStateName", () => Dao.Distinct<State>(x => x.GroupName, x => x.StateType == 0));
            RegisterParameterSource("TicketStateName", () => Dao.Distinct<State>(x => x.GroupName, x => x.StateType == 1));
            RegisterParameterSource("OrderStateName", () => Dao.Distinct<State>(x => x.GroupName, x => x.StateType == 2));
            RegisterParameterSource("EntityTypeName", () => Dao.Distinct<EntityType>(x => x.Name));
            RegisterParameterSource("AutomationCommandName", () => Dao.Distinct<AutomationCommand>(x => x.Name));
            RegisterParameterSource("PrintJobName", () => Dao.Distinct<PrintJob>(x => x.Name));
            RegisterParameterSource("PaymentTypeName", () => Dao.Distinct<PaymentType>(x => x.Name));
            RegisterParameterSource("AccountTransactionTypeName", () => Dao.Distinct<AccountTransactionType>(x => x.Name));
            RegisterParameterSource("AccountTransactionDocumentName", () => Dao.Distinct<AccountTransactionDocumentType>(x => x.Name));
            RegisterParameterSource("UpdateType", () => new[] { Resources.Update, Resources.Increase, Resources.Decrease, Resources.Toggle });
            RegisterParameterSource("TicketTypeName", () => Dao.Distinct<TicketType>(x => x.Name));
            RegisterParameterSource("EntityScreenName", () => Dao.Distinct<EntityScreen>(x => x.Name));
            RegisterParameterSource("PrinterTemplateName", () => Dao.Distinct<PrinterTemplate>(x => x.Name));
            RegisterParameterSource("PrinterName", () => Dao.Distinct<Printer>(x => x.Name));
            RegisterParameterSource("WidgetName", () => Dao.Distinct<Widget>(x => x.Name));
            RegisterParameterSource("AccountScreenName", () => Dao.Distinct<AccountScreen>(x => x.Name));
            RegisterParameterSource("AccountTransactionsFilter", () => new[] { Resources.Default, Resources.ThisMonth, Resources.PastMonth, Resources.ThisWeek, Resources.PastWeek, Resources.WorkPeriod });
        }
    }
}
