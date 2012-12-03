using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Management.Automation.Runspaces;
using System.Reflection;
using System.Windows.Media;
using Microsoft.Practices.ServiceLocation;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Automation;
using Samba.Domain.Models.Menus;
using Samba.Domain.Models.Resources;
using Samba.Domain.Models.Settings;
using Samba.Domain.Models.Tickets;
using Samba.Infrastructure.Helpers;
using Samba.Localization.Properties;
using Samba.Persistance.Data;
using Samba.Persistance.Data.Specification;
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
        private static readonly IResourceService ResourceService = ServiceLocator.Current.GetInstance<IResourceService>();
        private static readonly IMethodQueue MethodQueue = ServiceLocator.Current.GetInstance<IMethodQueue>();
        private static readonly ICacheService CacheService = ServiceLocator.Current.GetInstance<ICacheService>();
        private static readonly IExpressionService ExpressionService = ServiceLocator.Current.GetInstance<IExpressionService>();
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
            AutomationService.RegisterActionType(ActionNames.SendEmail, Resources.SendEmail, new { SMTPServer = "", SMTPUser = "", SMTPPassword = "", SMTPPort = 0, ToEMailAddress = "", Subject = "", CCEmailAddresses = "", FromEMailAddress = "", EMailMessage = "", FileName = "", DeleteFile = false, BypassSslErrors = false });
            AutomationService.RegisterActionType(ActionNames.AddOrder, Resources.AddOrder, new { MenuItemName = "", PortionName = "", Quantity = 0, Tag = "" });
            AutomationService.RegisterActionType(ActionNames.UpdateTicketTag, Resources.UpdateTicketTag, new { TagName = "", TagValue = "" });
            AutomationService.RegisterActionType(ActionNames.TagOrder, Resources.TagOrder, new { OrderTagName = "", OldOrderTagValue = "", OrderTagValue = "", OrderTagNote = "" });
            AutomationService.RegisterActionType(ActionNames.UntagOrder, Resources.UntagOrder, new { OrderTagName = "", OrderTagValue = "" });
            AutomationService.RegisterActionType(ActionNames.RemoveOrderTag, Resources.RemoveOrderTag, new { OrderTagName = "" });
            AutomationService.RegisterActionType(ActionNames.MoveTaggedOrders, Resources.MoveTaggedOrders, new { OrderTagName = "", OrderTagValue = "" });
            AutomationService.RegisterActionType(ActionNames.UpdateOrder, Resources.UpdateOrder, new { Quantity = 0m, Price = 0m, IncreaseInventory = false, DecreaseInventory = false, CalculatePrice = false, Locked = false, AccountTransactionType = "" });
            AutomationService.RegisterActionType(ActionNames.UpdatePriceTag, Resources.UpdatePriceTag, new { DepartmentName = "", PriceTag = "" });
            AutomationService.RegisterActionType(ActionNames.RefreshCache, Resources.RefreshCache);
            AutomationService.RegisterActionType(ActionNames.SendMessage, Resources.BroadcastMessage, new { Command = "" });
            AutomationService.RegisterActionType(ActionNames.UpdateProgramSetting, Resources.UpdateProgramSetting, new { SettingName = "", SettingValue = "", UpdateType = Resources.Update, IsLocal = true });
            AutomationService.RegisterActionType(ActionNames.UpdateTicketTax, Resources.UpdateTicketTax, new { TaxTemplate = "" });
            AutomationService.RegisterActionType(ActionNames.RegenerateTicketTax, Resources.RegenerateTicketTax);
            AutomationService.RegisterActionType(ActionNames.UpdateTicketCalculation, Resources.UpdateTicketCalculation, new { CalculationType = "", Amount = 0m });
            AutomationService.RegisterActionType(ActionNames.UpdateTicketAccount, Resources.UpdateTicketAccount, new { AccountPhone = "", AccountName = "", Note = "" });
            AutomationService.RegisterActionType(ActionNames.ExecutePrintJob, Resources.ExecutePrintJob, new { PrintJobName = "", OrderStateName = "", OrderState = "", OrderStateValue = "", OrderTagName = "", OrderTagValue = "" });
            AutomationService.RegisterActionType(ActionNames.CloseActiveTicket, Resources.CloseTicket);
            AutomationService.RegisterActionType(ActionNames.LockTicket, Resources.LockTicket);
            AutomationService.RegisterActionType(ActionNames.UnlockTicket, Resources.UnlockTicket);
            AutomationService.RegisterActionType(ActionNames.CreateTicket, string.Format(Resources.Create_f, Resources.Ticket));
            AutomationService.RegisterActionType(ActionNames.DisplayTicket, Resources.DisplayTicket, new { TicketId = 0 });
            AutomationService.RegisterActionType(ActionNames.DisplayTicketList, Resources.DisplayTicketList, new { TicketTagName = "", TicketStateName = "" });
            AutomationService.RegisterActionType(ActionNames.DisplayPaymentScreen, Resources.DisplayPaymentScreen);
            AutomationService.RegisterActionType(ActionNames.ExecutePowershellScript, Resources.ExecutePowershellScript, new { Script = "" });
            AutomationService.RegisterActionType(ActionNames.ExecuteScript, Resources.ExecuteScript, new { ScriptName = "" });
            AutomationService.RegisterActionType(ActionNames.UpdateResourceState, Resources.UpdateResourceState, new { ResourceTypeName = "", StateName = "", CurrentState = "", State = "" });
            AutomationService.RegisterActionType(ActionNames.UpdateTicketState, Resources.UpdateTicketState, new { StateName = "", CurrentState = "", State = "", StateValue = "", Quantity = 0 });
            AutomationService.RegisterActionType(ActionNames.UpdateOrderState, Resources.UpdateOrderState, new { StateName = "", GroupOrder = 0, CurrentState = "", State = "", StateOrder = 0, StateValue = "" });
        }

        private static void RegisterRules()
        {
            AutomationService.RegisterEvent(RuleEventNames.ApplicationScreenChanged, Resources.ApplicationScreenChanged, new { PreviousScreen = "", CurrentScreen = "" });
            AutomationService.RegisterEvent(RuleEventNames.UserLoggedIn, Resources.UserLogin, new { RoleName = "" });
            AutomationService.RegisterEvent(RuleEventNames.UserLoggedOut, Resources.UserLogout, new { RoleName = "" });
            AutomationService.RegisterEvent(RuleEventNames.WorkPeriodStarts, Resources.WorkPeriodStarted);
            AutomationService.RegisterEvent(RuleEventNames.BeforeWorkPeriodEnds, Resources.BeforeWorkPeriodEnds);
            AutomationService.RegisterEvent(RuleEventNames.WorkPeriodEnds, Resources.WorkPeriodEnded);
            AutomationService.RegisterEvent(RuleEventNames.TriggerExecuted, Resources.TriggerExecuted, new { TriggerName = "" });
            AutomationService.RegisterEvent(RuleEventNames.TicketCreated, Resources.TicketCreated);
            AutomationService.RegisterEvent(RuleEventNames.TicketOpened, Resources.TicketOpened, new { OrderCount = 0 });
            AutomationService.RegisterEvent(RuleEventNames.TicketClosing, Resources.TicketClosing, new { TicketId = 0, NewOrderCount = 0 });
            AutomationService.RegisterEvent(RuleEventNames.TicketsMerged, Resources.TicketsMerged);
            AutomationService.RegisterEvent(RuleEventNames.PaymentProcessed, Resources.PaymentProcessed, new { PaymentTypeName = "", TenderedAmount = 0m, ProcessedAmount = 0m, ChangeAmount = 0m, RemainingAmount = 0m, SelectedQuantity = 0m });
            AutomationService.RegisterEvent(RuleEventNames.TicketResourceChanged, Resources.TicketResourceChanged, new { OrderCount = 0, OldResourceName = "", NewResourceName = "" });
            AutomationService.RegisterEvent(RuleEventNames.TicketTagSelected, Resources.TicketTagSelected, new { TagName = "", TagValue = "", NumericValue = 0, TicketTag = "" });
            AutomationService.RegisterEvent(RuleEventNames.TicketStateUpdated, Resources.TicketStateUpdated, new { StateName = "", State = "", StateValue = "", Quantity = 0, TicketState = "" });
            AutomationService.RegisterEvent(RuleEventNames.ResourceStateUpdated, Resources.ResourceStateUpdated, new { StateName = "", State = "", ResourceState = "" });
            AutomationService.RegisterEvent(RuleEventNames.OrderTagged, Resources.OrderTagged, new { OrderTagName = "", OrderTagValue = "" });
            AutomationService.RegisterEvent(RuleEventNames.OrderUntagged, Resources.OrderUntagged, new { OrderTagName = "", OrderTagValue = "" });
            AutomationService.RegisterEvent(RuleEventNames.OrderStateUpdated, Resources.OrderStateUpdated, new { StateName = "", State = "", StateValue = "" });
            AutomationService.RegisterEvent(RuleEventNames.AccountSelectedForTicket, Resources.AccountSelectedForTicket, new { AccountName = "", PhoneNumber = "", AccountNote = "" });
            AutomationService.RegisterEvent(RuleEventNames.TicketTotalChanged, Resources.TicketTotalChanged, new { TicketTotal = 0m, PreviousTotal = 0m, DiscountTotal = 0m, DiscountAmount = 0m, TipAmount = 0m });
            AutomationService.RegisterEvent(RuleEventNames.MessageReceived, Resources.MessageReceived, new { Command = "" });
            AutomationService.RegisterEvent(RuleEventNames.TicketLineAdded, Resources.OrderAddedToTicket, new { MenuItemName = "" });
            AutomationService.RegisterEvent(RuleEventNames.ChangeAmountChanged, Resources.ChangeAmountUpdated, new { TicketAmount = 0, ChangeAmount = 0, TenderedAmount = 0 });
            AutomationService.RegisterEvent(RuleEventNames.ApplicationStarted, Resources.ApplicationStarted);
            AutomationService.RegisterEvent(RuleEventNames.ResourceUpdated, Resources.ResourceUpdated, new { ResourceTypeName = "", OpenTicketCount = 0 });
            AutomationService.RegisterEvent(RuleEventNames.AutomationCommandExecuted, Resources.AutomationCommandExecuted, new { AutomationCommandName = "", Value = "" });
        }

        private static void RegisterParameterSources()
        {
            AutomationService.RegisterParameterSoruce("UserName", () => UserService.GetUserNames());
            AutomationService.RegisterParameterSoruce("DepartmentName", () => DepartmentService.GetDepartmentNames());
            AutomationService.RegisterParameterSoruce("TerminalName", () => SettingService.GetTerminalNames());
            AutomationService.RegisterParameterSoruce("TriggerName", () => Dao.Select<Trigger, string>(yz => yz.Name, y => !string.IsNullOrEmpty(y.Expression)));
            AutomationService.RegisterParameterSoruce("MenuItemName", () => Dao.Distinct<MenuItem>(yz => yz.Name));
            AutomationService.RegisterParameterSoruce("PriceTag", () => Dao.Distinct<MenuItemPriceDefinition>(x => x.PriceTag));
            AutomationService.RegisterParameterSoruce("Color", () => typeof(Colors).GetProperties(BindingFlags.Public | BindingFlags.Static).Select(x => x.Name));
            AutomationService.RegisterParameterSoruce("TaxTemplate", () => Dao.Distinct<TaxTemplate>(x => x.Name));
            AutomationService.RegisterParameterSoruce("CalculationType", () => Dao.Distinct<CalculationType>(x => x.Name));
            AutomationService.RegisterParameterSoruce("TagName", () => Dao.Distinct<TicketTagGroup>(x => x.Name));
            AutomationService.RegisterParameterSoruce("OrderTagName", () => Dao.Distinct<OrderTagGroup>(x => x.Name));
            AutomationService.RegisterParameterSoruce("ResourceState", () => Dao.Distinct<ResourceState>(x => x.Name));
            AutomationService.RegisterParameterSoruce("ResourceTypeName", () => Dao.Distinct<ResourceType>(x => x.EntityName));
            AutomationService.RegisterParameterSoruce("AutomationCommandName", () => Dao.Distinct<AutomationCommand>(x => x.Name));
            AutomationService.RegisterParameterSoruce("PrintJobName", () => Dao.Distinct<PrintJob>(x => x.Name));
            AutomationService.RegisterParameterSoruce("PaymentTypeName", () => Dao.Distinct<PaymentType>(x => x.Name));
            AutomationService.RegisterParameterSoruce("AccountTransactionDocumentName", () => Dao.Distinct<AccountTransactionDocumentType>(x => x.Name));
        }

        private static void ResetCache()
        {
            TriggerService.UpdateCronObjects();
            EventServiceFactory.EventService.PublishEvent(EventTopicNames.ResetCache, true);
            ApplicationState.CurrentDepartment.PublishEvent(EventTopicNames.SelectedDepartmentChanged);
        }

        private static void HandleEvents()
        {
            EventServiceFactory.EventService.GetEvent<GenericEvent<IActionData>>().Subscribe(x =>
            {
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
                                                                                   CacheService.
                                                                                       GetAccountTransactionTypeIdByName
                                                                                       (x.Value.GetAsString(
                                                                                           "AccountTransactionType")));
                        }
                    }
                }

                if (x.Value.Action.ActionType == ActionNames.ExecuteScript)
                {
                    var script = x.Value.GetAsString("ScriptName");
                    if (!string.IsNullOrEmpty(script))
                    {
                        ExpressionService.EvalCommand(script, null, x.Value.DataObject, true);
                    }
                }

                if (x.Value.Action.ActionType == ActionNames.ExecutePowershellScript)
                {
                    var script = x.Value.GetAsString("Script");
                    if (!string.IsNullOrEmpty(script))
                    {
                        if (Utility.IsValidFile(script)) script = File.ReadAllText(script);
                        var runspace = RunspaceFactory.CreateRunspace();
                        runspace.Open();
                        runspace.SessionStateProxy.SetVariable("locator", ServiceLocator.Current);
                        var pipeline = runspace.CreatePipeline(script);
                        pipeline.Invoke();
                        runspace.Close();
                    }
                }

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
                        var dt = new TicketStateData() { StateName = ticketStateName };
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

                if (x.Value.Action.ActionType == ActionNames.UpdateResourceState)
                {
                    //var resource = x.Value.GetDataValue<Resource>("Resource");
                    //var stateName = x.Value.GetDataValueAsString("StateName");
                    //var currentState = x.Value.GetDataValueAsString("CurrentState");
                    //var state = x.Value.GetDataValueAsString("State");
                    //if (resource != null)
                    //{
                    //    ResourceService.UpdateResourceState2(resource, stateName, currentState, state);
                    //}
                    //else
                    //{
                    //    var ticket = x.Value.GetDataValue<Ticket>("Ticket");
                    //    if (ticket != null)
                    //    {
                    //        var resourceTypeName = x.Value.GetDataValueAsString("ResourceTypeName");
                    //        var resourceTypeId = CacheService.GetResourceTypeIdByEntityName(resourceTypeName);
                    //        foreach (var ticketResource in ticket.TicketResources)
                    //        {
                    //            resource = CacheService.GetResourceById(ticketResource.ResourceId);
                    //            if (resource.ResourceTypeId == resourceTypeId)
                    //                ResourceService.UpdateResourceState2(resource, stateName, currentState, state);
                    //        }
                    //    }
                    //}

                    var resourceId = x.Value.GetDataValueAsInt("ResourceId");
                    var resourceTypeId = x.Value.GetDataValueAsInt("ResourceTypeId");
                    var stateName = x.Value.GetAsString("StateName");
                    var state = x.Value.GetAsString("State");
                    if (state != null)
                    {
                        if (resourceId > 0 && resourceTypeId > 0)
                        {
                            ResourceService.UpdateResourceState(resourceId, stateName, state);
                        }
                        else
                        {
                            var ticket = x.Value.GetDataValue<Ticket>("Ticket");
                            if (ticket != null)
                            {
                                var resourceTypeName = x.Value.GetDataValueAsString("ResourceTypeName");
                                foreach (var ticketResource in ticket.TicketResources)
                                {
                                    var resourceType = CacheService.GetResourceTypeById(ticketResource.ResourceTypeId);
                                    if (string.IsNullOrEmpty(resourceTypeName.Trim()) || resourceType.Name == resourceTypeName)
                                        ResourceService.UpdateResourceState(ticketResource.ResourceId, stateName, state);
                                }
                            }
                        }
                    }
                }

                if (x.Value.Action.ActionType == ActionNames.UpdateTicketAccount)
                {
                    var ticket = x.Value.GetDataValue<Ticket>("Ticket");
                    if (ticket != null)
                    {
                        Expression<Func<Resource, bool>> qFilter = null;

                        var phoneNumber = x.Value.GetAsString("AccountPhone");
                        var accountName = x.Value.GetAsString("AccountName");
                        var note = x.Value.GetAsString("Note");

                        if (!string.IsNullOrEmpty(phoneNumber))
                        {
                            qFilter = y => y.SearchString == phoneNumber;
                        }

                        if (!string.IsNullOrEmpty(accountName))
                        {
                            if (qFilter == null) qFilter = y => y.Name == accountName;
                            else qFilter = qFilter.And(y => y.Name == accountName);
                        }

                        if (qFilter != null)
                        {
                            var resource = Dao.Query(qFilter).FirstOrDefault();
                            if (resource != null)
                                TicketService.UpdateResource(ticket, resource);
                        }
                        else TicketService.UpdateResource(ticket, Resource.Null);
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
                        else if (updateType == "Toggle")
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

                if (x.Value.Action.ActionType == ActionNames.UpdateTicketTax)
                {
                    var ticket = x.Value.GetDataValue<Ticket>("Ticket");
                    if (ticket != null)
                    {
                        var taxTemplateName = x.Value.GetAsString("TaxTemplate");
                        var taxTemplate = SettingService.GetTaxTemplateByName(taxTemplateName);
                        if (taxTemplate != null)
                        {
                            ticket.UpdateTax(taxTemplate);
                            TicketService.RecalculateTicket(ticket);
                            EventServiceFactory.EventService.PublishEvent(EventTopicNames.RefreshSelectedTicket);
                        }
                    }
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

                if (x.Value.Action.ActionType == ActionNames.RegenerateTicketTax)
                {
                    var ticket = x.Value.GetDataValue<Ticket>("Ticket");
                    if (ticket != null)
                    {
                        TicketService.RegenerateTaxRates(ticket);
                        TicketService.RecalculateTicket(ticket);
                        EventServiceFactory.EventService.PublishEvent(EventTopicNames.RefreshSelectedTicket);
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

                            //foreach (var order in orders)
                            //{
                            //    if (x.Value.Action.ActionType == ActionNames.RemoveOrderTag)
                            //    {
                            //        var tags = order.OrderTagValues.Where(y => y.OrderTagGroupId == orderTag.Id);
                            //        tags.ToList().ForEach(y => order.OrderTagValues.Remove(y));
                            //        continue;
                            //    }
                            //}

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
