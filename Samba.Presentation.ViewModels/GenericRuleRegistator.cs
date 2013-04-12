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
        private static readonly IAutomationServiceBase AutomationServiceBase = ServiceLocator.Current.GetInstance<IAutomationServiceBase>();
        private static readonly IAutomationService AutomationService = ServiceLocator.Current.GetInstance<IAutomationService>();
        private static readonly IEntityService EntityService = ServiceLocator.Current.GetInstance<IEntityService>();
        private static readonly IMethodQueue MethodQueue = ServiceLocator.Current.GetInstance<IMethodQueue>();
        private static readonly ICacheService CacheService = ServiceLocator.Current.GetInstance<ICacheService>();
        private static readonly IEmailService EmailService = ServiceLocator.Current.GetInstance<IEmailService>();

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
            AutomationServiceBase.RegisterActionType(ActionNames.SendEmail, Resources.SendEmail, new { SMTPServer = "", SMTPUser = "", SMTPPassword = "", SMTPPort = 0, ToEMailAddress = "", Subject = "", CCEmailAddresses = "", FromEMailAddress = "", EMailMessage = "", FileName = "", DeleteFile = false, BypassSslErrors = false });
            AutomationServiceBase.RegisterActionType(ActionNames.AddOrder, Resources.AddOrder, new { MenuItemName = "", PortionName = "", Quantity = 0, Tag = "" });
            AutomationServiceBase.RegisterActionType(ActionNames.SetActiveTicketType, Resources.SetActiveTicketType, new { TicketTypeName = "" });
            AutomationServiceBase.RegisterActionType(ActionNames.TagOrder, Resources.TagOrder, new { OrderTagName = "", OldOrderTagValue = "", OrderTagValue = "", OrderTagNote = "" });
            AutomationServiceBase.RegisterActionType(ActionNames.UntagOrder, Resources.UntagOrder, new { OrderTagName = "", OrderTagValue = "" });
            AutomationServiceBase.RegisterActionType(ActionNames.RemoveOrderTag, Resources.RemoveOrderTag, new { OrderTagName = "" });
            AutomationServiceBase.RegisterActionType(ActionNames.MoveTaggedOrders, Resources.MoveTaggedOrders, new { OrderTagName = "", OrderTagValue = "" });
            AutomationServiceBase.RegisterActionType(ActionNames.UpdateOrder, Resources.UpdateOrder, new { Quantity = 0m, Price = 0m, PortionName = "", PriceTag = "", IncreaseInventory = false, DecreaseInventory = false, CalculatePrice = false, Locked = false, AccountTransactionType = "" });
            AutomationServiceBase.RegisterActionType(ActionNames.UpdateOrderState, Resources.UpdateOrderState, new { StateName = "", GroupOrder = 0, CurrentState = "", State = "", StateOrder = 0, StateValue = "" });
            AutomationServiceBase.RegisterActionType(ActionNames.UpdateEntityState, Resources.UpdateEntityState, new { EntityTypeName = "", EntityStateName = "", CurrentState = "", EntityState = "" });
            AutomationServiceBase.RegisterActionType(ActionNames.UpdateProgramSetting, Resources.UpdateProgramSetting, new { SettingName = "", SettingValue = "", UpdateType = Resources.Update, IsLocal = true });
            AutomationServiceBase.RegisterActionType(ActionNames.UpdateTicketTag, Resources.UpdateTicketTag, new { TagName = "", TagValue = "" });
            AutomationServiceBase.RegisterActionType(ActionNames.ChangeTicketEntity, Resources.ChangeTicketEntity, new { EntityTypeName = "", EntityName = "" });
            AutomationServiceBase.RegisterActionType(ActionNames.UpdateTicketCalculation, Resources.UpdateTicketCalculation, new { CalculationType = "", Amount = 0m });
            AutomationServiceBase.RegisterActionType(ActionNames.UpdateTicketState, Resources.UpdateTicketState, new { StateName = "", CurrentState = "", State = "", StateValue = "", Quantity = 0 });
            AutomationServiceBase.RegisterActionType(ActionNames.CloseActiveTicket, Resources.CloseTicket);
            AutomationServiceBase.RegisterActionType(ActionNames.CreateTicket, string.Format(Resources.Create_f, Resources.Ticket));
            AutomationServiceBase.RegisterActionType(ActionNames.DisplayTicket, Resources.DisplayTicket, new { TicketId = 0 });
            AutomationServiceBase.RegisterActionType(ActionNames.DisplayTicketList, Resources.DisplayTicketList, new { TicketTagName = "", TicketStateName = "" });
            AutomationServiceBase.RegisterActionType(ActionNames.LockTicket, Resources.LockTicket);
            AutomationServiceBase.RegisterActionType(ActionNames.UnlockTicket, Resources.UnlockTicket);
            AutomationServiceBase.RegisterActionType(ActionNames.DisplayPaymentScreen, Resources.DisplayPaymentScreen);
            AutomationServiceBase.RegisterActionType(ActionNames.UpdatePriceTag, Resources.UpdatePriceTag, new { DepartmentName = "", PriceTag = "" });
            AutomationServiceBase.RegisterActionType(ActionNames.RefreshCache, Resources.RefreshCache);
            AutomationServiceBase.RegisterActionType(ActionNames.ExecutePrintJob, Resources.ExecutePrintJob, new { PrintJobName = "", OrderStateName = "", OrderState = "", OrderStateValue = "", OrderTagName = "", OrderTagValue = "" });
            AutomationServiceBase.RegisterActionType(ActionNames.SendMessage, Resources.BroadcastMessage, new { Command = "" });
            //AutomationService.RegisterActionType(ActionNames.ExecutePowershellScript, Resources.ExecutePowershellScript, new { Script = "" });
        }

        private static void RegisterRules()
        {
            AutomationServiceBase.RegisterEvent(RuleEventNames.ApplicationScreenChanged, Resources.ApplicationScreenChanged, new { PreviousScreen = "", CurrentScreen = "" });
            AutomationServiceBase.RegisterEvent(RuleEventNames.AutomationCommandExecuted, Resources.AutomationCommandExecuted, new { AutomationCommandName = "", Value = "" });
            AutomationServiceBase.RegisterEvent(RuleEventNames.TriggerExecuted, Resources.TriggerExecuted, new { TriggerName = "" });
            AutomationServiceBase.RegisterEvent(RuleEventNames.UserLoggedIn, Resources.UserLogin, new { RoleName = "" });
            AutomationServiceBase.RegisterEvent(RuleEventNames.UserLoggedOut, Resources.UserLogout, new { RoleName = "" });
            AutomationServiceBase.RegisterEvent(RuleEventNames.WorkPeriodStarts, Resources.WorkPeriodStarted);
            AutomationServiceBase.RegisterEvent(RuleEventNames.BeforeWorkPeriodEnds, Resources.BeforeWorkPeriodEnds);
            AutomationServiceBase.RegisterEvent(RuleEventNames.WorkPeriodEnds, Resources.WorkPeriodEnded);
            AutomationServiceBase.RegisterEvent(RuleEventNames.TicketCreated, Resources.TicketCreated, new { TicketTypeName = "" });
            AutomationServiceBase.RegisterEvent(RuleEventNames.TicketMoving, Resources.Ticket_Moving);
            AutomationServiceBase.RegisterEvent(RuleEventNames.TicketMoved, Resources.TicketMoved);
            AutomationServiceBase.RegisterEvent(RuleEventNames.TicketOpened, Resources.TicketOpened, new { OrderCount = 0 });
            AutomationServiceBase.RegisterEvent(RuleEventNames.TicketClosing, Resources.TicketClosing, new { TicketId = 0 });
            AutomationServiceBase.RegisterEvent(RuleEventNames.TicketsMerged, Resources.TicketsMerged);
            AutomationServiceBase.RegisterEvent(RuleEventNames.TicketEntityChanged, Resources.TicketEntityChanged, new { EntityTypeName = "", OrderCount = 0, OldEntityName = "", NewEntityName = "" });
            AutomationServiceBase.RegisterEvent(RuleEventNames.TicketTagSelected, Resources.TicketTagSelected, new { TagName = "", TagValue = "", NumericValue = 0, TicketTag = "" });
            AutomationServiceBase.RegisterEvent(RuleEventNames.TicketStateUpdated, Resources.TicketStateUpdated, new { StateName = "", State = "", StateValue = "", Quantity = 0, TicketState = "" });
            AutomationServiceBase.RegisterEvent(RuleEventNames.TicketTotalChanged, Resources.TicketTotalChanged, new { PreviousTotal = 0m, TicketTotal = 0m, RemainingAmount = 0m, DiscountTotal = 0m, PaymentTotal = 0m });
            AutomationServiceBase.RegisterEvent(RuleEventNames.PaymentProcessed, Resources.PaymentProcessed, new { PaymentTypeName = "", TenderedAmount = 0m, ProcessedAmount = 0m, ChangeAmount = 0m, RemainingAmount = 0m, SelectedQuantity = 0m });
            AutomationServiceBase.RegisterEvent(RuleEventNames.ChangeAmountChanged, Resources.ChangeAmountUpdated, new { TicketAmount = 0, ChangeAmount = 0, TenderedAmount = 0 });
            AutomationServiceBase.RegisterEvent(RuleEventNames.OrderAdded, Resources.OrderAddedToTicket, new { MenuItemName = "" });
            AutomationServiceBase.RegisterEvent(RuleEventNames.OrderMoved, Resources.OrderMoved, new { MenuItemName = "" });
            AutomationServiceBase.RegisterEvent(RuleEventNames.OrderTagged, Resources.OrderTagged, new { OrderTagName = "", OrderTagValue = "" });
            AutomationServiceBase.RegisterEvent(RuleEventNames.OrderUntagged, Resources.OrderUntagged, new { OrderTagName = "", OrderTagValue = "" });
            AutomationServiceBase.RegisterEvent(RuleEventNames.OrderStateUpdated, Resources.OrderStateUpdated, new { StateName = "", State = "", StateValue = "" });
            AutomationServiceBase.RegisterEvent(RuleEventNames.EntitySelected, Resources.EntitySelected, new { EntityTypeName = "", EntityName = "", EntityCustomData = "", IsTicketSelected = false });
            AutomationServiceBase.RegisterEvent(RuleEventNames.EntityUpdated, Resources.EntityUpdated, new { EntityTypeName = "", OpenTicketCount = 0 });
            AutomationServiceBase.RegisterEvent(RuleEventNames.EntityStateUpdated, Resources.EntityStateUpdated, new { EntityTypeName = "", StateName = "", State = "" });
            AutomationServiceBase.RegisterEvent(RuleEventNames.MessageReceived, Resources.MessageReceived, new { Command = "" });
            AutomationServiceBase.RegisterEvent(RuleEventNames.ApplicationStarted, Resources.ApplicationStarted);
        }

        private static void RegisterParameterSources()
        {
            AutomationServiceBase.RegisterParameterSoruce("UserName", () => UserService.GetUserNames());
            AutomationServiceBase.RegisterParameterSoruce("DepartmentName", () => DepartmentService.GetDepartmentNames());
            AutomationServiceBase.RegisterParameterSoruce("TerminalName", () => SettingService.GetTerminalNames());
            AutomationServiceBase.RegisterParameterSoruce("TriggerName", () => Dao.Select<Trigger, string>(yz => yz.Name, y => !string.IsNullOrEmpty(y.Expression)));
            AutomationServiceBase.RegisterParameterSoruce("MenuItemName", () => Dao.Distinct<MenuItem>(yz => yz.Name));
            AutomationServiceBase.RegisterParameterSoruce("PriceTag", () => Dao.Distinct<MenuItemPriceDefinition>(x => x.PriceTag));
            AutomationServiceBase.RegisterParameterSoruce("Color", () => typeof(Colors).GetProperties(BindingFlags.Public | BindingFlags.Static).Select(x => x.Name));
            AutomationServiceBase.RegisterParameterSoruce("TaxTemplate", () => Dao.Distinct<TaxTemplate>(x => x.Name));
            AutomationServiceBase.RegisterParameterSoruce("CalculationType", () => Dao.Distinct<CalculationType>(x => x.Name));
            AutomationServiceBase.RegisterParameterSoruce("TagName", () => Dao.Distinct<TicketTagGroup>(x => x.Name));
            AutomationServiceBase.RegisterParameterSoruce("OrderTagName", () => Dao.Distinct<OrderTagGroup>(x => x.Name));
            AutomationServiceBase.RegisterParameterSoruce("State", () => Dao.Distinct<State>(x => x.Name));
            AutomationServiceBase.RegisterParameterSoruce("EntityState", () => Dao.Distinct<State>(x => x.Name, x => x.StateType == 0));
            AutomationServiceBase.RegisterParameterSoruce("TicketState", () => Dao.Distinct<State>(x => x.Name, x => x.StateType == 1));
            AutomationServiceBase.RegisterParameterSoruce("OrderState", () => Dao.Distinct<State>(x => x.Name, x => x.StateType == 2));
            AutomationServiceBase.RegisterParameterSoruce("StateName", () => Dao.Distinct<State>(x => x.GroupName));
            AutomationServiceBase.RegisterParameterSoruce("EntityStateName", () => Dao.Distinct<State>(x => x.GroupName, x => x.StateType == 0));
            AutomationServiceBase.RegisterParameterSoruce("TicketStateName", () => Dao.Distinct<State>(x => x.GroupName, x => x.StateType == 1));
            AutomationServiceBase.RegisterParameterSoruce("OrderStateName", () => Dao.Distinct<State>(x => x.GroupName, x => x.StateType == 2));
            AutomationServiceBase.RegisterParameterSoruce("EntityTypeName", () => Dao.Distinct<EntityType>(x => x.Name));
            AutomationServiceBase.RegisterParameterSoruce("AutomationCommandName", () => Dao.Distinct<AutomationCommand>(x => x.Name));
            AutomationServiceBase.RegisterParameterSoruce("PrintJobName", () => Dao.Distinct<PrintJob>(x => x.Name));
            AutomationServiceBase.RegisterParameterSoruce("PaymentTypeName", () => Dao.Distinct<PaymentType>(x => x.Name));
            AutomationServiceBase.RegisterParameterSoruce("AccountTransactionTypeName", () => Dao.Distinct<AccountTransactionType>(x => x.Name));
            AutomationServiceBase.RegisterParameterSoruce("AccountTransactionDocumentName", () => Dao.Distinct<AccountTransactionDocumentType>(x => x.Name));
            AutomationServiceBase.RegisterParameterSoruce("UpdateType", () => new[] { Resources.Update, Resources.Increase, Resources.Decrease, Resources.Toggle });
            AutomationServiceBase.RegisterParameterSoruce("TicketTypeName", () => Dao.Distinct<TicketType>(x => x.Name));
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
            EventServiceFactory.EventService.GetEvent<GenericEvent<IActionData>>().Subscribe(x =>
            {
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

                if (x.Value.Action.ActionType == ActionNames.DisplayTicket)
                {
                    var ticketId = x.Value.GetAsInteger("TicketId");
                    if (ticketId > 0)
                        ExtensionMethods.PublishIdEvent(ticketId, EventTopicNames.DisplayTicket);
                    else EventServiceFactory.EventService.PublishEvent(EventTopicNames.RefreshSelectedTicket);
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
                    AppServices.MessagingService.SendMessage("ActionMessage", x.Value.GetAsString("Command"));
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
                        var calculationType = SettingService.GetCalculationTypeByName(calculationTypeName);
                        if (calculationType != null)
                        {
                            var amount = x.Value.GetAsDecimal("Amount");
                            ticket.AddCalculation(calculationType, amount);
                            TicketService.RecalculateTicket(ticket);
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
                        var tagData = new TicketTagData { TagName = tagName, TagValue = tagValue };
                        tagData.PublishEvent(EventTopicNames.TicketTagSelected);
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

        private static IList<Order> GetOrders(IActionData x, Ticket ticket)
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
                    AutomationService.NotifyEvent(RuleEventNames.MessageReceived, new { Command = x.Value.Data });
                }
            });
        }
    }
}
