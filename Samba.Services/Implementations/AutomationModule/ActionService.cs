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
    [Export(typeof(IActionService))]
    public class ActionService : IActionService
    {
        private readonly IAutomationService _automationService;
        private readonly IDeviceService _deviceService;

        [ImportMany]
        public IEnumerable<IActionProcessor> ActionProcessors { get; set; }

        [ImportingConstructor]
        public ActionService(IAutomationService automationService, IDeviceService deviceService)
        {
            _automationService = automationService;
            _deviceService = deviceService;
        }

        public void ProcessAction(string actionType, ActionData actionData)
        {
            var actionProcessor = ActionProcessors.FirstOrDefault(x => x.Handles(actionType));
            if (actionProcessor != null) actionProcessor.Process(actionData);
        }

        public void Register()
        {
            RegisterRules();
            RegisterActions();
            RegisterParameterSources();
        }

        private void RegisterActions()
        {
            foreach (var actionProcessor in ActionProcessors)
            {
                _automationService.RegisterActionType(actionProcessor.ActionKey, actionProcessor.ActionName, actionProcessor.DefaultData);
            }
        }

        private void RegisterRules()
        {
            _automationService.RegisterEvent(RuleEventNames.ApplicationScreenChanged, Resources.ApplicationScreenChanged, new { PreviousScreen = "", CurrentScreen = "" });
            _automationService.RegisterEvent(RuleEventNames.AutomationCommandExecuted, Resources.AutomationCommandExecuted, new { AutomationCommandName = "", Value = "" });
            _automationService.RegisterEvent(RuleEventNames.TriggerExecuted, Resources.TriggerExecuted, new { TriggerName = "" });
            _automationService.RegisterEvent(RuleEventNames.UserLoggedIn, Resources.UserLogin, new { RoleName = "" });
            _automationService.RegisterEvent(RuleEventNames.UserLoggedOut, Resources.UserLogout, new { RoleName = "" });
            _automationService.RegisterEvent(RuleEventNames.WorkPeriodStarts, Resources.WorkPeriodStarted);
            _automationService.RegisterEvent(RuleEventNames.BeforeWorkPeriodEnds, Resources.BeforeWorkPeriodEnds);
            _automationService.RegisterEvent(RuleEventNames.WorkPeriodEnds, Resources.WorkPeriodEnded);
            _automationService.RegisterEvent(RuleEventNames.TicketCreated, Resources.TicketCreated, new { TicketTypeName = "" });
            _automationService.RegisterEvent(RuleEventNames.TicketMoving, Resources.Ticket_Moving);
            _automationService.RegisterEvent(RuleEventNames.TicketMoved, Resources.TicketMoved);
            _automationService.RegisterEvent(RuleEventNames.TicketOpened, Resources.TicketOpened, new { OrderCount = 0 });
            _automationService.RegisterEvent(RuleEventNames.BeforeTicketClosing, Resources.BeforeTicketClosing, new { TicketId = 0, RemainingAmount = 0m, TotalAmount = 0m });
            _automationService.RegisterEvent(RuleEventNames.TicketClosing, Resources.TicketClosing, new { TicketId = 0, RemainingAmount = 0m, TotalAmount = 0m });
            _automationService.RegisterEvent(RuleEventNames.TicketsMerged, Resources.TicketsMerged);
            _automationService.RegisterEvent(RuleEventNames.TicketEntityChanged, Resources.TicketEntityChanged, new { EntityTypeName = "", OrderCount = 0, OldEntityName = "", NewEntityName = "", CustomData = "" });
            _automationService.RegisterEvent(RuleEventNames.TicketTagSelected, Resources.TicketTagSelected, new { TagName = "", TagValue = "", NumericValue = 0, TicketTag = "" });
            _automationService.RegisterEvent(RuleEventNames.TicketStateUpdated, Resources.TicketStateUpdated, new { StateName = "", State = "", StateValue = "", Quantity = 0, TicketState = "" });
            _automationService.RegisterEvent(RuleEventNames.TicketTotalChanged, Resources.TicketTotalChanged, new { PreviousTotal = 0m, TicketTotal = 0m, RemainingAmount = 0m, DiscountTotal = 0m, PaymentTotal = 0m });
            _automationService.RegisterEvent(RuleEventNames.PaymentProcessed, Resources.PaymentProcessed, new { PaymentTypeName = "", TenderedAmount = 0m, ProcessedAmount = 0m, ChangeAmount = 0m, RemainingAmount = 0m, SelectedQuantity = 0m });
            _automationService.RegisterEvent(RuleEventNames.ChangeAmountChanged, Resources.ChangeAmountUpdated, new { TicketAmount = 0, ChangeAmount = 0, TenderedAmount = 0 });
            _automationService.RegisterEvent(RuleEventNames.OrderAdded, Resources.OrderAddedToTicket, new { MenuItemGroupCode = "", MenuItemTag = "", MenuItemName = "" });
            _automationService.RegisterEvent(RuleEventNames.OrderMoved, Resources.OrderMoved, new { MenuItemName = "" });
            _automationService.RegisterEvent(RuleEventNames.OrderTagged, Resources.OrderTagged, new { OrderTagName = "", OrderTagValue = "" });
            _automationService.RegisterEvent(RuleEventNames.OrderUntagged, Resources.OrderUntagged, new { OrderTagName = "", OrderTagValue = "" });
            _automationService.RegisterEvent(RuleEventNames.OrderStateUpdated, Resources.OrderStateUpdated, new { StateName = "", State = "", StateValue = "" });
            _automationService.RegisterEvent(RuleEventNames.EntitySelected, Resources.EntitySelected, new { EntityTypeName = "", EntityName = "", EntityCustomData = "", IsTicketSelected = false });
            _automationService.RegisterEvent(RuleEventNames.EntityUpdated, Resources.EntityUpdated, new { EntityTypeName = "", OpenTicketCount = 0 });
            _automationService.RegisterEvent(RuleEventNames.EntityStateUpdated, Resources.EntityStateUpdated, new { EntityTypeName = "", StateName = "", State = "", Quantity = 0m });
            _automationService.RegisterEvent(RuleEventNames.AccountTransactionDocumentCreated, Resources.AccountTransactionDocumentCreated, new { AccountTransactionDocumentName = "", DocumentId = 0 });
            _automationService.RegisterEvent(RuleEventNames.MessageReceived, Resources.MessageReceived, new { Command = "" });
            _automationService.RegisterEvent(RuleEventNames.DeviceEventGenerated, Resources.DeviceEventGenerated, new { DeviceName = "", EventName = "", EventData = "" });
            _automationService.RegisterEvent(RuleEventNames.ApplicationStarted, Resources.ApplicationStarted);
            _automationService.RegisterEvent(RuleEventNames.ValueLooped, Resources.ValueLooped, new { Name = "", Value = "" });
            _automationService.RegisterEvent(RuleEventNames.NumberpadValueEntered, Resources.NumberpadValueEntered, new { Value = "" });
        }

        private void RegisterParameterSources()
        {
            _automationService.RegisterParameterSource("UserName", () => Dao.Distinct<User>(x => x.Name));
            _automationService.RegisterParameterSource("DepartmentName", () => Dao.Distinct<Department>(x => x.Name));
            _automationService.RegisterParameterSource("TerminalName", () => Dao.Distinct<Terminal>(x => x.Name));
            _automationService.RegisterParameterSource("DeviceName", () => _deviceService.GetDeviceNames());
            _automationService.RegisterParameterSource("TriggerName", () => Dao.Select<Trigger, string>(yz => yz.Name, y => !string.IsNullOrEmpty(y.Expression)));
            _automationService.RegisterParameterSource("MenuItemName", () => Dao.Distinct<MenuItem>(yz => yz.Name));
            _automationService.RegisterParameterSource("PriceTag", () => Dao.Distinct<MenuItemPriceDefinition>(x => x.PriceTag));
            _automationService.RegisterParameterSource("Color", () => typeof(Colors).GetProperties(BindingFlags.Public | BindingFlags.Static).Select(x => x.Name));
            _automationService.RegisterParameterSource("TaxTemplate", () => Dao.Distinct<TaxTemplate>(x => x.Name));
            _automationService.RegisterParameterSource("CalculationType", () => Dao.Distinct<CalculationType>(x => x.Name));
            _automationService.RegisterParameterSource("TagName", () => Dao.Distinct<TicketTagGroup>(x => x.Name));
            _automationService.RegisterParameterSource("OrderTagName", () => Dao.Distinct<OrderTagGroup>(x => x.Name));
            _automationService.RegisterParameterSource("State", () => Dao.Distinct<State>(x => x.Name));
            _automationService.RegisterParameterSource("EntityState", () => Dao.Distinct<State>(x => x.Name, x => x.StateType == 0));
            _automationService.RegisterParameterSource("TicketState", () => Dao.Distinct<State>(x => x.Name, x => x.StateType == 1));
            _automationService.RegisterParameterSource("OrderState", () => Dao.Distinct<State>(x => x.Name, x => x.StateType == 2));
            _automationService.RegisterParameterSource("StateName", () => Dao.Distinct<State>(x => x.GroupName));
            _automationService.RegisterParameterSource("EntityStateName", () => Dao.Distinct<State>(x => x.GroupName, x => x.StateType == 0));
            _automationService.RegisterParameterSource("TicketStateName", () => Dao.Distinct<State>(x => x.GroupName, x => x.StateType == 1));
            _automationService.RegisterParameterSource("OrderStateName", () => Dao.Distinct<State>(x => x.GroupName, x => x.StateType == 2));
            _automationService.RegisterParameterSource("EntityTypeName", () => Dao.Distinct<EntityType>(x => x.Name));
            _automationService.RegisterParameterSource("AutomationCommandName", () => Dao.Distinct<AutomationCommand>(x => x.Name));
            _automationService.RegisterParameterSource("PrintJobName", () => Dao.Distinct<PrintJob>(x => x.Name));
            _automationService.RegisterParameterSource("PaymentTypeName", () => Dao.Distinct<PaymentType>(x => x.Name));
            _automationService.RegisterParameterSource("AccountTransactionTypeName", () => Dao.Distinct<AccountTransactionType>(x => x.Name));
            _automationService.RegisterParameterSource("AccountTransactionDocumentName", () => Dao.Distinct<AccountTransactionDocumentType>(x => x.Name));
            _automationService.RegisterParameterSource("UpdateType", () => new[] { Resources.Update, Resources.Increase, Resources.Decrease, Resources.Toggle });
            _automationService.RegisterParameterSource("TicketTypeName", () => Dao.Distinct<TicketType>(x => x.Name));
            _automationService.RegisterParameterSource("EntityScreenName", () => Dao.Distinct<EntityScreen>(x => x.Name));
            _automationService.RegisterParameterSource("PrinterTemplateName", () => Dao.Distinct<PrinterTemplate>(x => x.Name));
            _automationService.RegisterParameterSource("PrinterName", () => Dao.Distinct<Printer>(x => x.Name));
            _automationService.RegisterParameterSource("WidgetName", () => Dao.Distinct<Widget>(x => x.Name));
        }
    }
}
