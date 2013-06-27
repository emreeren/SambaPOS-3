using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
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
using Samba.Persistance.Specification;
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
        private static readonly ITicketService TicketService = ServiceLocator.Current.GetInstance<ITicketService>();
        private static readonly IApplicationState ApplicationState = ServiceLocator.Current.GetInstance<IApplicationState>();
        private static readonly IUserService UserService = ServiceLocator.Current.GetInstance<IUserService>();
        private static readonly ITriggerService TriggerService = ServiceLocator.Current.GetInstance<ITriggerService>();
        private static readonly IPrinterService PrinterService = ServiceLocator.Current.GetInstance<IPrinterService>();
        private static readonly ISettingService SettingService = ServiceLocator.Current.GetInstance<ISettingService>();
        private static readonly IAutomationService AutomationService = ServiceLocator.Current.GetInstance<IAutomationService>();
        private static readonly IEntityServiceClient EntityService = ServiceLocator.Current.GetInstance<IEntityServiceClient>();
        private static readonly IMethodQueue MethodQueue = ServiceLocator.Current.GetInstance<IMethodQueue>();
        private static readonly ICacheService CacheService = ServiceLocator.Current.GetInstance<ICacheService>();
        private static readonly IEmailService EmailService = ServiceLocator.Current.GetInstance<IEmailService>();
        private static readonly IDeviceService DeviceService = ServiceLocator.Current.GetInstance<IDeviceService>();
        private static readonly IMessagingService MessagingService = ServiceLocator.Current.GetInstance<IMessagingService>();

        private static bool _registered;

        public static void RegisterOnce()
        {
            Debug.Assert(_registered == false);
            RegisterActions();
            RegisterRules();
            RegisterParameterSources();
            HandleEvents();
            RegisterNotifiers();
            _registered = true;
        }

        private static void RegisterActions()
        {
            AutomationService.RegisterActionType(ActionNames.SendEmail, Resources.SendEmail, new { SMTPServer = "", SMTPUser = "", SMTPPassword = "", SMTPPort = 0, ToEMailAddress = "", Subject = "", CCEmailAddresses = "", FromEMailAddress = "", EMailMessage = "", FileName = "", DeleteFile = false, BypassSslErrors = false });
            AutomationService.RegisterActionType(ActionNames.AddOrder, Resources.AddOrder, new { MenuItemName = "", PortionName = "", Quantity = 0, Tag = "" });
            AutomationService.RegisterActionType(ActionNames.SetActiveTicketType, Resources.SetActiveTicketType, new { TicketTypeName = "" });
            AutomationService.RegisterActionType(ActionNames.TagOrder, Resources.TagOrder, new { OrderTagName = "", OldOrderTagValue = "", OrderTagValue = "", OrderTagNote = "", OrderTagPrice = "" });
            AutomationService.RegisterActionType(ActionNames.UntagOrder, Resources.UntagOrder, new { OrderTagName = "", OrderTagValue = "" });
            AutomationService.RegisterActionType(ActionNames.RemoveOrderTag, Resources.RemoveOrderTag, new { OrderTagName = "" });
            AutomationService.RegisterActionType(ActionNames.MoveTaggedOrders, Resources.MoveTaggedOrders, new { OrderTagName = "", OrderTagValue = "" });
            AutomationService.RegisterActionType(ActionNames.UpdateOrder, Resources.UpdateOrder, new { Quantity = 0m, Price = 0m, PortionName = "", PriceTag = "", IncreaseInventory = false, DecreaseInventory = false, CalculatePrice = false, Locked = false, AccountTransactionType = "" });
            AutomationService.RegisterActionType(ActionNames.UpdateOrderState, Resources.UpdateOrderState, new { StateName = "", GroupOrder = 0, CurrentState = "", State = "", StateOrder = 0, StateValue = "" });
            AutomationService.RegisterActionType(ActionNames.UpdateEntityState, Resources.UpdateEntityState, new { EntityTypeName = "", EntityStateName = "", CurrentState = "", EntityState = "" });
            AutomationService.RegisterActionType(ActionNames.UpdateProgramSetting, Resources.UpdateProgramSetting, new { SettingName = "", SettingValue = "", UpdateType = Resources.Update, IsLocal = true });
            AutomationService.RegisterActionType(ActionNames.UpdateTicketTag, Resources.UpdateTicketTag, new { TagName = "", TagValue = "" });
            AutomationService.RegisterActionType(ActionNames.ChangeTicketEntity, Resources.ChangeTicketEntity, new { EntityTypeName = "", EntityName = "" });
            AutomationService.RegisterActionType(ActionNames.UpdateTicketCalculation, Resources.UpdateTicketCalculation, new { CalculationType = "", Amount = 0m });
            AutomationService.RegisterActionType(ActionNames.UpdateTicketState, Resources.UpdateTicketState, new { StateName = "", CurrentState = "", State = "", StateValue = "", Quantity = 0 });
            AutomationService.RegisterActionType(ActionNames.CloseActiveTicket, Resources.CloseTicket);
            AutomationService.RegisterActionType(ActionNames.LoadTicket, Resources.LoadTicket, new { TicketId = 0 });
            AutomationService.RegisterActionType(ActionNames.CreateTicket, string.Format(Resources.Create_f, Resources.Ticket));
            AutomationService.RegisterActionType(ActionNames.DisplayTicket, Resources.DisplayTicket, new { TicketId = 0 });
            AutomationService.RegisterActionType(ActionNames.DisplayTicketList, Resources.DisplayTicketList, new { TicketTagName = "", TicketStateName = "" });
            AutomationService.RegisterActionType(ActionNames.LockTicket, Resources.LockTicket);
            AutomationService.RegisterActionType(ActionNames.UnlockTicket, Resources.UnlockTicket);
            AutomationService.RegisterActionType(ActionNames.MarkTicketAsClosed, Resources.MarkTicketAsClosed);
            AutomationService.RegisterActionType(ActionNames.DisplayPaymentScreen, Resources.DisplayPaymentScreen);
            AutomationService.RegisterActionType(ActionNames.UpdatePriceTag, Resources.UpdatePriceTag, new { DepartmentName = "", PriceTag = "" });
            AutomationService.RegisterActionType(ActionNames.RefreshCache, Resources.RefreshCache);
            AutomationService.RegisterActionType(ActionNames.ExecutePrintJob, Resources.ExecutePrintJob, new { PrintJobName = "", OrderStateName = "", OrderState = "", OrderStateValue = "", OrderTagName = "", OrderTagValue = "" });
            AutomationService.RegisterActionType(ActionNames.SendMessage, Resources.BroadcastMessage, new { Command = "" });
            AutomationService.RegisterActionType(ActionNames.StartProcess, Resources.StartProcess, new { FileName = "", Arguments = "", UseShellExecute = false, IsHidden = false });
            AutomationService.RegisterActionType(ActionNames.LoopValues, "Loop Values", new { Name = "", Values = "" });
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
            AutomationService.RegisterEvent(RuleEventNames.EntityStateUpdated, Resources.EntityStateUpdated, new { EntityTypeName = "", StateName = "", State = "" });
            AutomationService.RegisterEvent(RuleEventNames.MessageReceived, Resources.MessageReceived, new { Command = "" });
            AutomationService.RegisterEvent(RuleEventNames.DeviceEventGenerated, Resources.DeviceEventGenerated, new { DeviceName = "", EventName = "", EventData = "" });
            AutomationService.RegisterEvent(RuleEventNames.ApplicationStarted, Resources.ApplicationStarted);
            AutomationService.RegisterEvent(RuleEventNames.ValueLooped, "Value Looped", new { Name = "", Value = "" });
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
        }

        private static void ResetCache()
        {
            TriggerService.UpdateCronObjects();
            EventServiceFactory.EventService.PublishEvent(EventTopicNames.ResetCache, true);
            ApplicationState.CurrentDepartment.PublishEvent(EventTopicNames.SelectedDepartmentChanged);
            ApplicationState.CurrentTicketType.PublishEvent(EventTopicNames.TicketTypeChanged);
        }

        private static void HandleEvents()
        {
            EventServiceFactory.EventService.GetEvent<GenericEvent<ActionData>>().Subscribe(x =>
            {
                if (x.Value.Action.ActionType == ActionNames.LoopValues)
                {
                    var name = x.Value.GetAsString("Name");
                    var values = x.Value.GetAsString("Values");
                    if (!string.IsNullOrEmpty(values))
                    {
                        foreach (var value in values.Split(','))
                        {
                            ApplicationState.NotifyEvent(RuleEventNames.ValueLooped, new { Name = name, Value = value });
                        }
                    }
                }

                if (x.Value.Action.ActionType == ActionNames.StartProcess)
                {
                    var fileName = x.Value.GetAsString("FileName");
                    var arguments = x.Value.GetAsString("Arguments");
                    if (!string.IsNullOrEmpty(fileName))
                    {
                        var psi = new ProcessStartInfo(fileName, arguments);
                        var isHidden = x.Value.GetAsBoolean("IsHidden");
                        if (isHidden) psi.WindowStyle = ProcessWindowStyle.Hidden;

                        var useShellExecute = x.Value.GetAsBoolean("UseShellExecute");
                        if (useShellExecute) psi.UseShellExecute = true;

                        var workingDirectory = x.Value.GetAsString("WorkingDirectory");
                        if (!string.IsNullOrEmpty(workingDirectory))
                            psi.WorkingDirectory = workingDirectory;

                        Process.Start(psi);
                    }
                }

                if (x.Value.Action.ActionType == ActionNames.SetActiveTicketType)
                {
                    var ticketTypeName = x.Value.GetAsString("TicketTypeName");
                    var ticketType = CacheService.GetTicketTypes().SingleOrDefault(y => y.Name == ticketTypeName);
                    if (ticketType != null)
                    {
                        ApplicationState.TempTicketType = ticketType;
                    }
                    else if (ApplicationState.SelectedEntityScreen != null && ApplicationState.SelectedEntityScreen.TicketTypeId != 0)
                    {
                        ApplicationState.TempTicketType = CacheService.GetTicketTypeById(ApplicationState.SelectedEntityScreen.TicketTypeId);
                    }
                    else
                    {
                        ApplicationState.TempTicketType = CacheService.GetTicketTypeById(ApplicationState.CurrentDepartment.TicketTypeId);
                    }
                }

                if (x.Value.Action.ActionType == ActionNames.ChangeTicketEntity)
                {
                    var ticket = x.Value.GetDataValue<Ticket>("Ticket");
                    if (ticket != null)
                    {
                        var entityTypeName = x.Value.GetAsString("EntityTypeName");
                        var entityName = x.Value.GetAsString("EntityName");
                        if (!string.IsNullOrEmpty(entityTypeName))
                        {
                            var entity = CacheService.GetEntityByName(entityTypeName, entityName);
                            if (entity != null)
                                TicketService.UpdateEntity(ticket, entity);
                        }
                    }
                }

                if (x.Value.Action.ActionType == ActionNames.UpdateOrder)
                {
                    var ticket = x.Value.GetDataValue<Ticket>("Ticket");
                    var orders = GetOrders(x.Value, ticket);
                    if (orders.Any())
                    {
                        foreach (var order in orders)
                        {
                            if (!string.IsNullOrEmpty(x.Value.GetAsString("Quantity")))
                                order.Quantity = x.Value.GetAsDecimal("Quantity");
                            if (!string.IsNullOrEmpty(x.Value.GetAsString("Price")))
                                order.UpdatePrice(x.Value.GetAsDecimal("Price"), "");
                            if (!string.IsNullOrEmpty(x.Value.GetAsString("IncreaseInventory")))
                                order.IncreaseInventory = x.Value.GetAsBoolean("IncreaseInventory");
                            if (!string.IsNullOrEmpty(x.Value.GetAsString("DecreaseInventory")))
                                order.DecreaseInventory = x.Value.GetAsBoolean("DecreaseInventory");
                            if (!string.IsNullOrEmpty(x.Value.GetAsString("Locked")))
                                order.Locked = x.Value.GetAsBoolean("Locked");
                            if (!string.IsNullOrEmpty(x.Value.GetAsString("CalculatePrice")))
                                order.CalculatePrice = x.Value.GetAsBoolean("CalculatePrice");
                            if (!string.IsNullOrEmpty(x.Value.GetAsString("AccountTransactionType")))
                                TicketService.ChangeOrdersAccountTransactionTypeId(ticket, new List<Order> { order },
                                                                                   CacheService.GetAccountTransactionTypeIdByName
                                                                                       (x.Value.GetAsString("AccountTransactionType")));

                            if (!string.IsNullOrEmpty(x.Value.GetAsString("PortionName")) || !string.IsNullOrEmpty(x.Value.GetAsString("PriceTag")))
                            {
                                var portionName = x.Value.GetAsString("PortionName");
                                var priceTag = x.Value.GetAsString("PriceTag");
                                TicketService.UpdateOrderPrice(order, portionName, priceTag);
                            }
                        }
                    }
                }

                // Not supported on XP machines. We'll move it to a module later

                //if (x.Value.Action.ActionType == ActionNames.ExecutePowershellScript)
                //{
                //    var script = x.Value.GetAsString("Script");
                //    if (!string.IsNullOrEmpty(script))
                //    {
                //        if (Utility.IsValidFile(script)) script = File.ReadAllText(script);
                //        var runspace = RunspaceFactory.CreateRunspace();
                //        runspace.Open();
                //        runspace.SessionStateProxy.SetVariable("locator", ServiceLocator.Current);
                //        var pipeline = runspace.CreatePipeline(script);
                //        pipeline.Invoke();
                //        runspace.Close();
                //    }
                //}

                if (x.Value.Action.ActionType == ActionNames.DisplayPaymentScreen)
                {
                    var ticket = x.Value.GetDataValue<Ticket>("Ticket");
                    if (ticket != null)
                    {
                        ticket.PublishEvent(EventTopicNames.MakePayment);
                    }
                }

                if (x.Value.Action.ActionType == ActionNames.LockTicket)
                {
                    var ticket = x.Value.GetDataValue<Ticket>("Ticket");
                    if (ticket != null)
                    {
                        ticket.RequestLock();
                    }
                }

                if (x.Value.Action.ActionType == ActionNames.LoadTicket)
                {
                    var ticketId = x.Value.GetAsInteger("TicketId");
                    var ticket = TicketService.OpenTicket(ticketId);
                    x.Value.DataObject.Ticket = ticket;
                    ticket.PublishEvent(EventTopicNames.SetSelectedTicket);
                }

                if (x.Value.Action.ActionType == ActionNames.DisplayTicket)
                {
                    var ticketId = x.Value.GetAsInteger("TicketId");
                    if (ticketId > 0)
                        ExtensionMethods.PublishIdEvent(ticketId, EventTopicNames.DisplayTicket);
                    else
                    {
                        if (ApplicationState.IsLocked)
                            EventServiceFactory.EventService.PublishEvent(EventTopicNames.RefreshSelectedTicket);
                    }
                }

                if (x.Value.Action.ActionType == ActionNames.DisplayTicketList)
                {
                    var ticketTagName = x.Value.GetAsString("TicketTagName");
                    var ticketStateName = x.Value.GetAsString("TicketStateName");

                    if (!string.IsNullOrEmpty(ticketStateName))
                    {
                        var dt = new TicketStateData { StateName = ticketStateName };
                        dt.PublishEvent(EventTopicNames.ActivateTicketList);
                    }
                    else if (!string.IsNullOrEmpty(ticketTagName))
                    {
                        var dt = new TicketTagGroup { Name = ticketTagName };
                        dt.PublishEvent(EventTopicNames.ActivateTicketList);
                    }
                }

                if (x.Value.Action.ActionType == ActionNames.CreateTicket)
                {
                    EventServiceFactory.EventService.PublishEvent(EventTopicNames.CreateTicket);
                }

                if (x.Value.Action.ActionType == ActionNames.UnlockTicket)
                {
                    var ticket = x.Value.GetDataValue<Ticket>("Ticket");
                    if (ticket != null) ticket.UnLock();
                    EventServiceFactory.EventService.PublishEvent(EventTopicNames.UnlockTicketRequested);
                }

                if (x.Value.Action.ActionType == ActionNames.MarkTicketAsClosed)
                {
                    var ticket = x.Value.GetDataValue<Ticket>("Ticket");
                    if (ticket != null) ticket.Close();
                }

                if (x.Value.Action.ActionType == ActionNames.CloseActiveTicket)
                {
                    EventServiceFactory.EventService.PublishEvent(EventTopicNames.CloseTicketRequested, true);
                }

                if (x.Value.Action.ActionType == ActionNames.UpdateEntityState)
                {
                    var entityId = x.Value.GetDataValueAsInt("EntityId");
                    var entityTypeId = x.Value.GetDataValueAsInt("EntityTypeId");
                    var stateName = x.Value.GetAsString("EntityStateName");
                    var state = x.Value.GetAsString("EntityState");
                    if (state != null)
                    {
                        if (entityId > 0 && entityTypeId > 0)
                        {
                            EntityService.UpdateEntityState(entityId, entityTypeId, stateName, state);
                        }
                        else
                        {
                            var ticket = x.Value.GetDataValue<Ticket>("Ticket");
                            if (ticket != null)
                            {
                                var entityTypeName = x.Value.GetDataValueAsString("EntityTypeName");
                                foreach (var ticketEntity in ticket.TicketEntities)
                                {
                                    var entityType = CacheService.GetEntityTypeById(ticketEntity.EntityTypeId);
                                    if (string.IsNullOrEmpty(entityTypeName.Trim()) || entityType.Name == entityTypeName)
                                        EntityService.UpdateEntityState(ticketEntity.EntityId, ticketEntity.EntityTypeId, stateName, state);
                                }
                            }
                        }
                    }
                }

                if (x.Value.Action.ActionType == ActionNames.UpdateProgramSetting)
                {
                    var settingName = x.Value.GetAsString("SettingName");
                    var updateType = x.Value.GetAsString("UpdateType");
                    if (!string.IsNullOrEmpty(settingName))
                    {
                        var isLocal = x.Value.GetAsBoolean("IsLocal");
                        var setting = isLocal
                            ? SettingService.ReadLocalSetting(settingName)
                            : SettingService.ReadGlobalSetting(settingName);

                        if (updateType == Resources.Increase)
                        {
                            var settingValue = x.Value.GetAsInteger("SettingValue");
                            if (string.IsNullOrEmpty(setting.StringValue))
                                setting.IntegerValue = settingValue;
                            else
                                setting.IntegerValue = setting.IntegerValue + settingValue;
                        }
                        else if (updateType == Resources.Decrease)
                        {
                            var settingValue = x.Value.GetAsInteger("SettingValue");
                            if (string.IsNullOrEmpty(setting.StringValue))
                                setting.IntegerValue = settingValue;
                            else
                                setting.IntegerValue = setting.IntegerValue - settingValue;
                        }
                        else if (updateType == Resources.Toggle)
                        {
                            var settingValue = x.Value.GetAsString("SettingValue");
                            var parts = settingValue.Split(',');
                            if (string.IsNullOrEmpty(setting.StringValue))
                            {
                                setting.StringValue = parts[0];
                            }
                            else
                            {
                                for (var i = 0; i < parts.Length; i++)
                                {
                                    if (parts[i] == setting.StringValue)
                                    {
                                        setting.StringValue = (i + 1) < parts.Length ? parts[i + 1] : parts[0];
                                        break;
                                    }
                                }
                            }
                        }
                        else
                        {
                            var settingValue = x.Value.GetAsString("SettingValue");
                            setting.StringValue = settingValue;
                        }
                        if (!isLocal) SettingService.SaveProgramSettings();
                    }
                }

                if (x.Value.Action.ActionType == ActionNames.RefreshCache)
                {
                    MethodQueue.Queue("ResetCache", ResetCache);
                }

                if (x.Value.Action.ActionType == ActionNames.SendMessage)
                {
                    MessagingService.SendMessage("ActionMessage", x.Value.GetAsString("Command"));
                }

                if (x.Value.Action.ActionType == ActionNames.SendEmail)
                {
                    EmailService.SendEMailAsync(x.Value.GetAsString("SMTPServer"),
                        x.Value.GetAsString("SMTPUser"),
                        x.Value.GetAsString("SMTPPassword"),
                        x.Value.GetAsInteger("SMTPPort"),
                        x.Value.GetAsString("ToEMailAddress"),
                        x.Value.GetAsString("CCEmailAddresses"),
                        x.Value.GetAsString("FromEMailAddress"),
                        x.Value.GetAsString("Subject"),
                        x.Value.GetAsString("EMailMessage"),
                        x.Value.GetAsString("FileName"),
                        x.Value.GetAsBoolean("DeleteFile"),
                        x.Value.GetAsBoolean("BypassSslErrors"));
                }

                if (x.Value.Action.ActionType == ActionNames.UpdateTicketCalculation)
                {
                    var ticket = x.Value.GetDataValue<Ticket>("Ticket");
                    if (ticket != null)
                    {
                        var calculationTypeName = x.Value.GetAsString("CalculationType");
                        var calculationType = CacheService.GetCalculationTypeByName(calculationTypeName);
                        if (calculationType != null)
                        {
                            var amount = x.Value.GetAsDecimal("Amount");
                            ticket.AddCalculation(calculationType, amount);
                            TicketService.RecalculateTicket(ticket);
                            EventServiceFactory.EventService.PublishEvent(EventTopicNames.RegenerateSelectedTicket);
                        }
                    }
                }

                if (x.Value.Action.ActionType == ActionNames.AddOrder)
                {
                    var ticket = x.Value.GetDataValue<Ticket>("Ticket");

                    if (ticket != null)
                    {
                        var menuItemName = x.Value.GetAsString("MenuItemName");
                        var menuItem = CacheService.GetMenuItem(y => y.Name == menuItemName);
                        var portionName = x.Value.GetAsString("PortionName");
                        var quantity = x.Value.GetAsDecimal("Quantity");
                        var tag = x.Value.GetAsString("Tag");
                        var order = TicketService.AddOrder(ticket, menuItem.Id, quantity, portionName, null);
                        if (order != null) order.Tag = tag;
                        order.PublishEvent(EventTopicNames.OrderAdded);
                    }
                }

                if (x.Value.Action.ActionType == ActionNames.UpdateTicketTag)
                {
                    var ticket = x.Value.GetDataValue<Ticket>("Ticket");
                    if (ticket != null)
                    {
                        var tagName = x.Value.GetAsString("TagName");
                        var tagValue = x.Value.GetAsString("TagValue");
                        ticket.SetTagValue(tagName, tagValue);
                    }
                }

                if (x.Value.Action.ActionType == ActionNames.UpdateTicketState)
                {
                    var ticket = x.Value.GetDataValue<Ticket>("Ticket");
                    if (ticket != null)
                    {
                        var stateName = x.Value.GetAsString("StateName");
                        var currentState = x.Value.GetAsString("CurrentState");
                        var state = x.Value.GetAsString("State");
                        var stateValue = x.Value.GetAsString("StateValue");
                        var quantity = x.Value.GetAsInteger("Quantity");
                        TicketService.UpdateTicketState(ticket, stateName, currentState, state, stateValue, quantity);
                    }
                }

                if (x.Value.Action.ActionType == ActionNames.UpdateOrderState)
                {
                    var ticket = x.Value.GetDataValue<Ticket>("Ticket");
                    var orders = GetOrders(x.Value, ticket);
                    if (orders.Any())
                    {
                        var stateName = x.Value.GetAsString("StateName");
                        var currentState = x.Value.GetAsString("CurrentState");
                        var groupOrder = x.Value.GetAsInteger("GroupOrder");
                        var state = x.Value.GetAsString("State");
                        var stateOrder = x.Value.GetAsInteger("StateOrder");
                        var stateValue = x.Value.GetAsString("StateValue");
                        TicketService.UpdateOrderStates(ticket, orders.ToList(), stateName, currentState, groupOrder, state, stateOrder, stateValue);
                    }
                }

                if (x.Value.Action.ActionType == ActionNames.TagOrder || x.Value.Action.ActionType == ActionNames.UntagOrder || x.Value.Action.ActionType == ActionNames.RemoveOrderTag)
                {
                    var ticket = x.Value.GetDataValue<Ticket>("Ticket");
                    var orders = GetOrders(x.Value, ticket);
                    if (orders.Any())
                    {
                        var tagName = x.Value.GetAsString("OrderTagName");
                        var orderTag = CacheService.GetOrderTagGroupByName(tagName);

                        if (orderTag != null)
                        {
                            var tagValue = x.Value.GetAsString("OrderTagValue");
                            var oldTagValue = x.Value.GetAsString("OldOrderTagValue");
                            var tagNote = x.Value.GetAsString("OrderTagNote");
                            var orderTagValue = orderTag.OrderTags.SingleOrDefault(y => y.Name == tagValue);
                            if (!string.IsNullOrEmpty(x.Value.GetAsString("OrderTagPrice")))
                            {
                                var price = x.Value.GetAsDecimal("OrderTagPrice");
                                orderTagValue.Price = price;
                            }
                            if (orderTagValue != null)
                            {
                                if (!string.IsNullOrEmpty(oldTagValue))
                                    orders = orders.Where(o => o.OrderTagExists(y => y.OrderTagGroupId == orderTag.Id && y.TagValue == oldTagValue)).ToList();
                                if (x.Value.Action.ActionType == ActionNames.TagOrder)
                                    TicketService.TagOrders(ticket, orders, orderTag, orderTagValue, tagNote);
                                if (x.Value.Action.ActionType == ActionNames.UntagOrder)
                                    TicketService.UntagOrders(ticket, orders, orderTag, orderTagValue);
                            }
                        }
                    }
                }

                if (x.Value.Action.ActionType == ActionNames.MoveTaggedOrders)
                {
                    var ticket = x.Value.GetDataValue<Ticket>("Ticket");
                    var orderTagName = x.Value.GetAsString("OrderTagName");
                    if (ticket != null && !string.IsNullOrEmpty(orderTagName))
                    {
                        var orderTagValue = x.Value.GetAsString("OrderTagValue");
                        if (ticket.Orders.Any(y => y.OrderTagExists(z => z.TagName == orderTagName && z.TagValue == orderTagValue)))
                        {
                            var tid = ticket.Id;
                            EventServiceFactory.EventService.PublishEvent(EventTopicNames.CloseTicketRequested, true);
                            ticket = TicketService.OpenTicket(tid);
                            var orders = ticket.Orders.Where(y => y.OrderTagExists(z => z.TagName == orderTagName && z.TagValue == orderTagValue)).ToArray();
                            var commitResult = TicketService.MoveOrders(ticket, orders, 0);
                            if (string.IsNullOrEmpty(commitResult.ErrorMessage) && commitResult.TicketId > 0)
                            {
                                ExtensionMethods.PublishIdEvent(commitResult.TicketId, EventTopicNames.DisplayTicket);
                            }
                            else
                            {
                                ExtensionMethods.PublishIdEvent(tid, EventTopicNames.DisplayTicket);
                            }
                        }
                    }
                }

                if (x.Value.Action.ActionType == ActionNames.UpdatePriceTag)
                {
                    using (var workspace = WorkspaceFactory.Create())
                    {
                        var priceTag = x.Value.GetAsString("PriceTag");
                        var departmentName = x.Value.GetAsString("DepartmentName");
                        var department = workspace.Single<Department>(y => y.Name == departmentName);
                        if (department != null)
                        {
                            department.PriceTag = priceTag;
                            workspace.CommitChanges();
                            MethodQueue.Queue("ResetCache", ResetCache);
                        }
                    }
                }

                if (x.Value.Action.ActionType == ActionNames.ExecutePrintJob)
                {
                    var ticket = x.Value.GetDataValue<Ticket>("Ticket");
                    var pjName = x.Value.GetAsString("PrintJobName");
                    if (!string.IsNullOrEmpty(pjName))
                    {
                        TicketService.UpdateTicketNumber(ticket, ApplicationState.CurrentTicketType.TicketNumerator);
                        var j = CacheService.GetPrintJobByName(pjName);

                        if (j != null)
                        {
                            if (ticket != null)
                            {
                                var orderTagName = x.Value.GetAsString("OrderTagName");
                                var orderTagValue = x.Value.GetAsString("OrderTagValue");
                                var orderStateName = x.Value.GetAsString("OrderStateName");
                                var orderState = x.Value.GetAsString("OrderState");
                                var orderStateValue = x.Value.GetAsString("OrderStateValue");
                                Expression<Func<Order, bool>> expression = ex => true;
                                if (!string.IsNullOrWhiteSpace(orderTagName))
                                {
                                    expression = ex => ex.OrderTagExists(y => y.TagName == orderTagName && y.TagValue == orderTagValue);
                                }
                                if (!string.IsNullOrWhiteSpace(orderStateName))
                                {
                                    expression = expression.And(ex => ex.IsInState(orderStateName, orderState));
                                    if (!string.IsNullOrWhiteSpace(orderStateValue))
                                        expression = expression.And(ex => ex.IsInState(orderStateValue));
                                }
                                PrinterService.PrintTicket(ticket, j, expression.Compile());
                            }
                            else
                                PrinterService.ExecutePrintJob(j);
                        }
                    }
                }
            });
        }

        private static IList<Order> GetOrders(ActionData x, Ticket ticket)
        {
            IList<Order> orders = new List<Order>();
            var selectedOrder = x.GetDataValue<Order>("Order");
            if (selectedOrder == null)
            {
                if (ticket != null)
                {
                    orders = ticket.Orders.Any(y => y.IsSelected)
                                 ? ticket.ExtractSelectedOrders().ToList()
                                 : ticket.Orders;
                }
            }
            else orders.Add(selectedOrder);
            return orders;
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
