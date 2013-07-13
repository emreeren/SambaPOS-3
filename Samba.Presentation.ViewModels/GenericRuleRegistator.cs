using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows.Media;
using Microsoft.Practices.ServiceLocation;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Automation;
using Samba.Domain.Models.Entities;
using Samba.Domain.Models.Menus;
using Samba.Domain.Models.Settings;
using Samba.Domain.Models.Tickets;
using Samba.Localization.Properties;
using Samba.Persistance.Data;
using Samba.Presentation.Common;
using Samba.Presentation.Services;
using Samba.Presentation.Services.Common;
using Samba.Services;
using Samba.Services.Common;

namespace Samba.Presentation.ViewModels
{
    public static class GenericRuleRegistator
    {
        private static readonly IDepartmentService DepartmentService = ServiceLocator.Current.GetInstance<IDepartmentService>();
        private static readonly IApplicationState ApplicationState = ServiceLocator.Current.GetInstance<IApplicationState>();
        private static readonly IUserService UserService = ServiceLocator.Current.GetInstance<IUserService>();
        private static readonly ISettingService SettingService = ServiceLocator.Current.GetInstance<ISettingService>();
        private static readonly IAutomationService AutomationService = ServiceLocator.Current.GetInstance<IAutomationService>();
        private static readonly IDeviceService DeviceService = ServiceLocator.Current.GetInstance<IDeviceService>();
        private static readonly IActionService ActionService = ServiceLocator.Current.GetInstance<IActionService>();

        private static bool _registered;

        public static void RegisterOnce()
        {
            Debug.Assert(_registered == false);
            ActionService.RegisterActions();
            RegisterRules();
            RegisterParameterSources();
            HandleEvents();
            RegisterNotifiers();
            _registered = true;
        }

        private static void RegisterRules()
        {
            AutomationService.RegisterEvent(RuleEventNames.ApplicationScreenChanged, Resources.ApplicationScreenChanged, new { PreviousScreen = "", CurrentScreen = "" });
            AutomationService.RegisterEvent(RuleEventNames.AutomationCommandExecuted, Resources.AutomationCommandExecuted, new { AutomationCommandName = "", Value = "" });
            AutomationService.RegisterEvent(RuleEventNames.TriggerExecuted, Resources.TriggerExecuted, new { TriggerName = "" });
            AutomationService.RegisterEvent(RuleEventNames.UserLoggedIn, Resources.UserLogin, new { RoleName = "" });
            AutomationService.RegisterEvent(RuleEventNames.UserLoggedOut, Resources.UserLogout, new { RoleName = "" });
            AutomationService.RegisterEvent(RuleEventNames.WorkPeriodStarts, Resources.WorkPeriodStarted);
            AutomationService.RegisterEvent(RuleEventNames.BeforeWorkPeriodEnds, Resources.BeforeWorkPeriodEnds);
            AutomationService.RegisterEvent(RuleEventNames.WorkPeriodEnds, Resources.WorkPeriodEnded);
            AutomationService.RegisterEvent(RuleEventNames.TicketCreated, Resources.TicketCreated, new { TicketTypeName = "" });
            AutomationService.RegisterEvent(RuleEventNames.TicketMoving, Resources.Ticket_Moving);
            AutomationService.RegisterEvent(RuleEventNames.TicketMoved, Resources.TicketMoved);
            AutomationService.RegisterEvent(RuleEventNames.TicketOpened, Resources.TicketOpened, new { OrderCount = 0 });
            AutomationService.RegisterEvent(RuleEventNames.BeforeTicketClosing, Resources.BeforeTicketClosing, new { TicketId = 0, RemainingAmount = 0m, TotalAmount = 0m });
            AutomationService.RegisterEvent(RuleEventNames.TicketClosing, Resources.TicketClosing, new { TicketId = 0, RemainingAmount = 0m, TotalAmount = 0m });
            AutomationService.RegisterEvent(RuleEventNames.TicketsMerged, Resources.TicketsMerged);
            AutomationService.RegisterEvent(RuleEventNames.TicketEntityChanged, Resources.TicketEntityChanged, new { EntityTypeName = "", OrderCount = 0, OldEntityName = "", NewEntityName = "", CustomData = "" });
            AutomationService.RegisterEvent(RuleEventNames.TicketTagSelected, Resources.TicketTagSelected, new { TagName = "", TagValue = "", NumericValue = 0, TicketTag = "" });
            AutomationService.RegisterEvent(RuleEventNames.TicketStateUpdated, Resources.TicketStateUpdated, new { StateName = "", State = "", StateValue = "", Quantity = 0, TicketState = "" });
            AutomationService.RegisterEvent(RuleEventNames.TicketTotalChanged, Resources.TicketTotalChanged, new { PreviousTotal = 0m, TicketTotal = 0m, RemainingAmount = 0m, DiscountTotal = 0m, PaymentTotal = 0m });
            AutomationService.RegisterEvent(RuleEventNames.PaymentProcessed, Resources.PaymentProcessed, new { PaymentTypeName = "", TenderedAmount = 0m, ProcessedAmount = 0m, ChangeAmount = 0m, RemainingAmount = 0m, SelectedQuantity = 0m });
            AutomationService.RegisterEvent(RuleEventNames.ChangeAmountChanged, Resources.ChangeAmountUpdated, new { TicketAmount = 0, ChangeAmount = 0, TenderedAmount = 0 });
            AutomationService.RegisterEvent(RuleEventNames.OrderAdded, Resources.OrderAddedToTicket, new { MenuItemGroupCode = "", MenuItemTag = "", MenuItemName = "" });
            AutomationService.RegisterEvent(RuleEventNames.OrderMoved, Resources.OrderMoved, new { MenuItemName = "" });
            AutomationService.RegisterEvent(RuleEventNames.OrderTagged, Resources.OrderTagged, new { OrderTagName = "", OrderTagValue = "" });
            AutomationService.RegisterEvent(RuleEventNames.OrderUntagged, Resources.OrderUntagged, new { OrderTagName = "", OrderTagValue = "" });
            AutomationService.RegisterEvent(RuleEventNames.OrderStateUpdated, Resources.OrderStateUpdated, new { StateName = "", State = "", StateValue = "" });
            AutomationService.RegisterEvent(RuleEventNames.EntitySelected, Resources.EntitySelected, new { EntityTypeName = "", EntityName = "", EntityCustomData = "", IsTicketSelected = false });
            AutomationService.RegisterEvent(RuleEventNames.EntityUpdated, Resources.EntityUpdated, new { EntityTypeName = "", OpenTicketCount = 0 });
            AutomationService.RegisterEvent(RuleEventNames.EntityStateUpdated, Resources.EntityStateUpdated, new { EntityTypeName = "", StateName = "", State = "", Quantity = 0m });
            AutomationService.RegisterEvent(RuleEventNames.AccountTransactionDocumentCreated, Resources.AccountTransactionDocumentCreated, new { AccountTransactionDocumentName = "", DocumentId = 0 });
            AutomationService.RegisterEvent(RuleEventNames.MessageReceived, Resources.MessageReceived, new { Command = "" });
            AutomationService.RegisterEvent(RuleEventNames.DeviceEventGenerated, Resources.DeviceEventGenerated, new { DeviceName = "", EventName = "", EventData = "" });
            AutomationService.RegisterEvent(RuleEventNames.ApplicationStarted, Resources.ApplicationStarted);
            AutomationService.RegisterEvent(RuleEventNames.ValueLooped, Resources.ValueLooped, new { Name = "", Value = "" });
            AutomationService.RegisterEvent(RuleEventNames.NumberpadValueEntered, Resources.NumberpadValueEntered, new { Value = "" });
        }

        private static void RegisterParameterSources()
        {
            AutomationService.RegisterParameterSource("UserName", () => UserService.GetUserNames());
            AutomationService.RegisterParameterSource("DepartmentName", () => DepartmentService.GetDepartmentNames());
            AutomationService.RegisterParameterSource("TerminalName", () => SettingService.GetTerminalNames());
            AutomationService.RegisterParameterSource("DeviceName", () => DeviceService.GetDeviceNames());
            AutomationService.RegisterParameterSource("TriggerName", () => Dao.Select<Trigger, string>(yz => yz.Name, y => !string.IsNullOrEmpty(y.Expression)));
            AutomationService.RegisterParameterSource("MenuItemName", () => Dao.Distinct<MenuItem>(yz => yz.Name));
            AutomationService.RegisterParameterSource("PriceTag", () => Dao.Distinct<MenuItemPriceDefinition>(x => x.PriceTag));
            AutomationService.RegisterParameterSource("Color", () => typeof(Colors).GetProperties(BindingFlags.Public | BindingFlags.Static).Select(x => x.Name));
            AutomationService.RegisterParameterSource("TaxTemplate", () => Dao.Distinct<TaxTemplate>(x => x.Name));
            AutomationService.RegisterParameterSource("CalculationType", () => Dao.Distinct<CalculationType>(x => x.Name));
            AutomationService.RegisterParameterSource("TagName", () => Dao.Distinct<TicketTagGroup>(x => x.Name));
            AutomationService.RegisterParameterSource("OrderTagName", () => Dao.Distinct<OrderTagGroup>(x => x.Name));
            AutomationService.RegisterParameterSource("State", () => Dao.Distinct<State>(x => x.Name));
            AutomationService.RegisterParameterSource("EntityState", () => Dao.Distinct<State>(x => x.Name, x => x.StateType == 0));
            AutomationService.RegisterParameterSource("TicketState", () => Dao.Distinct<State>(x => x.Name, x => x.StateType == 1));
            AutomationService.RegisterParameterSource("OrderState", () => Dao.Distinct<State>(x => x.Name, x => x.StateType == 2));
            AutomationService.RegisterParameterSource("StateName", () => Dao.Distinct<State>(x => x.GroupName));
            AutomationService.RegisterParameterSource("EntityStateName", () => Dao.Distinct<State>(x => x.GroupName, x => x.StateType == 0));
            AutomationService.RegisterParameterSource("TicketStateName", () => Dao.Distinct<State>(x => x.GroupName, x => x.StateType == 1));
            AutomationService.RegisterParameterSource("OrderStateName", () => Dao.Distinct<State>(x => x.GroupName, x => x.StateType == 2));
            AutomationService.RegisterParameterSource("EntityTypeName", () => Dao.Distinct<EntityType>(x => x.Name));
            AutomationService.RegisterParameterSource("AutomationCommandName", () => Dao.Distinct<AutomationCommand>(x => x.Name));
            AutomationService.RegisterParameterSource("PrintJobName", () => Dao.Distinct<PrintJob>(x => x.Name));
            AutomationService.RegisterParameterSource("PaymentTypeName", () => Dao.Distinct<PaymentType>(x => x.Name));
            AutomationService.RegisterParameterSource("AccountTransactionTypeName", () => Dao.Distinct<AccountTransactionType>(x => x.Name));
            AutomationService.RegisterParameterSource("AccountTransactionDocumentName", () => Dao.Distinct<AccountTransactionDocumentType>(x => x.Name));
            AutomationService.RegisterParameterSource("UpdateType", () => new[] { Resources.Update, Resources.Increase, Resources.Decrease, Resources.Toggle });
            AutomationService.RegisterParameterSource("TicketTypeName", () => Dao.Distinct<TicketType>(x => x.Name));
            AutomationService.RegisterParameterSource("EntityScreenName", () => Dao.Distinct<EntityScreen>(x => x.Name));
            AutomationService.RegisterParameterSource("PrinterTemplateName", () => Dao.Distinct<PrinterTemplate>(x => x.Name));
            AutomationService.RegisterParameterSource("PrinterName", () => Dao.Distinct<Printer>(x => x.Name));
            AutomationService.RegisterParameterSource("WidgetName", () => Dao.Distinct<Widget>(x => x.Name));
        }

        private static void HandleEvents()
        {
            EventServiceFactory.EventService.GetEvent<GenericEvent<ActionData>>().Subscribe(x =>
            {
                if (ActionService.CanProcessAction(x.Value.Action.ActionType))
                {
                    ActionService.ProcessAction(x.Value.Action.ActionType, x.Value);
                }
            });
        }


        private static void RegisterNotifiers()
        {
            EventServiceFactory.EventService.GetEvent<GenericEvent<Message>>().Subscribe(x =>
            {
                if (x.Topic == EventTopicNames.MessageReceivedEvent && x.Value.Command == "ActionMessage")
                {
                    ApplicationState.NotifyEvent(RuleEventNames.MessageReceived, new { Command = x.Value.Data });
                }
            });
        }
    }
}
