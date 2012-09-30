using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows.Media;
using Microsoft.Practices.ServiceLocation;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Actions;
using Samba.Domain.Models.Menus;
using Samba.Domain.Models.Resources;
using Samba.Domain.Models.Settings;
using Samba.Domain.Models.Tickets;
using Samba.Localization.Properties;
using Samba.Persistance.Data;
using Samba.Persistance.Data.Specification;
using Samba.Presentation.Common;
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
        private static readonly ICacheService CacheService = ServiceLocator.Current.GetInstance<ICacheService>();
        private static readonly IResourceService ResourceService = ServiceLocator.Current.GetInstance<IResourceService>();
        private static readonly IAccountService AccountService = ServiceLocator.Current.GetInstance<IAccountService>();

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
            AutomationService.RegisterActionType("SendEmail", Resources.SendEmail, new { SMTPServer = "", SMTPUser = "", SMTPPassword = "", SMTPPort = 0, ToEMailAddress = "", Subject = "", CCEmailAddresses = "", FromEMailAddress = "", EMailMessage = "", FileName = "", DeleteFile = false, BypassSslErrors = false });
            AutomationService.RegisterActionType("AddTicketDiscount", Resources.AddTicketDiscount, new { DiscountPercentage = 0m });
            AutomationService.RegisterActionType("AddOrder", Resources.AddOrder, new { MenuItemName = "", PortionName = "", Quantity = 0, Tag = "" });
            AutomationService.RegisterActionType("UpdateTicketTag", Resources.UpdateTicketTag, new { TagName = "", TagValue = "" });
            AutomationService.RegisterActionType("TagOrder", "Tag Order", new { OrderTagName = "", OrderTagValue = "" });
            AutomationService.RegisterActionType("UntagOrder", "Untag Order", new { OrderTagName = "", OrderTagValue = "" });
            AutomationService.RegisterActionType("RemoveOrderTag", "Remove OrderTag", new { OrderTagName = "" });
            AutomationService.RegisterActionType("MoveTaggedOrders", "Move Tagged Orders", new { OrderTagName = "", OrderTagValue = "" });
            AutomationService.RegisterActionType("UpdatePriceTag", Resources.UpdatePriceTag, new { DepartmentName = "", PriceTag = "" });
            AutomationService.RegisterActionType("RefreshCache", Resources.RefreshCache);
            AutomationService.RegisterActionType("SendMessage", Resources.BroadcastMessage, new { Command = "" });
            AutomationService.RegisterActionType("UpdateProgramSetting", Resources.UpdateProgramSetting, new { SettingName = "", SettingValue = "", UpdateType = Resources.Update, IsLocal = true });
            AutomationService.RegisterActionType("UpdateTicketTax", Resources.UpdateTicketTax, new { TaxTemplate = "" });
            AutomationService.RegisterActionType("RegenerateTicketTax", Resources.RegenerateTicketTax);
            AutomationService.RegisterActionType("UpdateTicketService", Resources.UpdateTicketService, new { CalculationType = "", Amount = 0m });
            AutomationService.RegisterActionType("UpdateTicketAccount", Resources.UpdateTicketAccount, new { AccountPhone = "", AccountName = "", Note = "" });
            AutomationService.RegisterActionType("ExecutePrintJob", "Execute Print Job", new { PrintJobName = "" });
            AutomationService.RegisterActionType("UpdateResourceState", "Update Resource State", new { ResourceTypeName = "", ResourceState = "" });
            AutomationService.RegisterActionType("CloseActiveTicket", "Close Active Ticket");
            AutomationService.RegisterActionType("LockTicket", "Lock Ticket");
            AutomationService.RegisterActionType("UnlockTicket", "Unlock Ticket");
            AutomationService.RegisterActionType("CreateTicket", "Create Ticket");
            AutomationService.RegisterActionType("DisplayTicket", "Display Ticket", new { TicketId = 0 });
            AutomationService.RegisterActionType("DisplayPaymentScreen", "Display Payment Screen");
            AutomationService.RegisterActionType("CreateAccountTransactionDocument", "Create Account Transaction Document", new { AccountTransactionDocumentName = "" });
        }

        private static void RegisterRules()
        {
            AutomationService.RegisterEvent(RuleEventNames.UserLoggedIn, Resources.UserLogin, new { RoleName = "" });
            AutomationService.RegisterEvent(RuleEventNames.UserLoggedOut, Resources.UserLogout, new { RoleName = "" });
            AutomationService.RegisterEvent(RuleEventNames.WorkPeriodStarts, Resources.WorkPeriodStarted);
            AutomationService.RegisterEvent(RuleEventNames.BeforeWorkPeriodEnds, "Before Work Period Ends");
            AutomationService.RegisterEvent(RuleEventNames.WorkPeriodEnds, Resources.WorkPeriodEnded);
            AutomationService.RegisterEvent(RuleEventNames.TriggerExecuted, Resources.TriggerExecuted, new { TriggerName = "" });
            AutomationService.RegisterEvent(RuleEventNames.TicketOpened, "Ticket Opened", new { OrderCount = 0 });
            AutomationService.RegisterEvent(RuleEventNames.TicketClosing, "Ticket Closing", new { TicketId = 0, NewOrderCount = 0 });
            AutomationService.RegisterEvent(RuleEventNames.TicketsMerged, "Tickets Merged");
            AutomationService.RegisterEvent(RuleEventNames.PaymentProcessed, "Payment Processed", new { PaymentTypeName = "", Tenderedamount = 0m, ProcessedAmount = 0m, ChangeAmount = 0m, RemainingAmount = 0m });
            AutomationService.RegisterEvent(RuleEventNames.TicketResourceChanged, "Ticket Resource Changed", new { OrderCount = 0, OldResourceName = "", NewResourceName = "" });
            AutomationService.RegisterEvent(RuleEventNames.TicketTagSelected, Resources.TicketTagSelected, new { TagName = "", TagValue = "", NumericValue = 0, TicketTag = "" });
            AutomationService.RegisterEvent(RuleEventNames.OrderTagged, "Order Tagged", new { OrderTagName = "", OrderTagValue = "" });
            AutomationService.RegisterEvent(RuleEventNames.OrderUntagged, "Order Untagged", new { OrderTagName = "", OrderTagValue = "" });
            AutomationService.RegisterEvent(RuleEventNames.AccountSelectedForTicket, Resources.AccountSelectedForTicket, new { AccountName = "", PhoneNumber = "", AccountNote = "" });
            AutomationService.RegisterEvent(RuleEventNames.TicketTotalChanged, Resources.TicketTotalChanged, new { TicketTotal = 0m, PreviousTotal = 0m, DiscountTotal = 0m, DiscountAmount = 0m, TipAmount = 0m });
            AutomationService.RegisterEvent(RuleEventNames.MessageReceived, Resources.MessageReceived, new { Command = "" });
            AutomationService.RegisterEvent(RuleEventNames.TicketLineAdded, "Line Added to Ticket", new { MenuItemName = "" });
            AutomationService.RegisterEvent(RuleEventNames.ChangeAmountChanged, "Change Amount Updated", new { TicketAmount = 0, ChangeAmount = 0, TenderedAmount = 0 });
            AutomationService.RegisterEvent(RuleEventNames.ApplicationStarted, "Application Started");
            AutomationService.RegisterEvent(RuleEventNames.ResourceUpdated, "Resource Updated", new { ResourceTypeName = "", OpenTicketCount = 0 });
            AutomationService.RegisterEvent(RuleEventNames.AutomationCommandExecuted, "Automation Command Exeucted", new { AutomationCommandName = "", Value = "" });
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
            AutomationService.RegisterParameterSoruce("ResourceTypeName", () => Dao.Distinct<ResourceType>(x => x.Name));
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
                if (x.Value.Action.ActionType == "CreateAccountTransactionDocument")
                {
                    var documentName = x.Value.GetAsString("AccountTransactionDocumentName");
                    if (!string.IsNullOrEmpty(documentName))
                    {
                        var document = CacheService.GetAccountTransactionDocumentTypeByName(documentName);
                        if (document != null)
                        {
                            var accounts = AccountService.GetDocumentAccounts(document);
                            foreach (var account in accounts)
                            {
                                var map = document.AccountTransactionDocumentAccountMaps.FirstOrDefault(
                                        y => y.AccountId == account.Id);
                                if (map != null && map.MappedAccountId > 0)
                                {
                                    var targetAccount = new Account { Id = map.MappedAccountId, Name = map.MappedAccountName };
                                    var amount = AccountService.GetDefaultAmount(document, account);
                                    if (amount != 0)
                                        AccountService.CreateNewTransactionDocument(account, document, "", amount, new List<Account> { targetAccount });
                                }
                            }
                        }
                    }
                }
                if (x.Value.Action.ActionType == "DisplayPaymentScreen")
                {
                    var ticket = x.Value.GetDataValue<Ticket>("Ticket");
                    if (ticket != null)
                    {
                        ticket.PublishEvent(EventTopicNames.MakePayment);
                    }
                }

                if (x.Value.Action.ActionType == "LockTicket")
                {
                    var ticket = x.Value.GetDataValue<Ticket>("Ticket");
                    if (ticket != null)
                    {
                        ticket.LockTicket();
                    }
                }

                if (x.Value.Action.ActionType == "DisplayTicket")
                {
                    var ticketId = x.Value.GetAsInteger("TicketId");
                    if (ticketId > 0)
                        ExtensionMethods.PublishIdEvent(ticketId, EventTopicNames.DisplayTicket);
                }

                if (x.Value.Action.ActionType == "CreateTicket")
                {
                    EventServiceFactory.EventService.PublishEvent(EventTopicNames.CreateTicket);
                }

                if (x.Value.Action.ActionType == "UnlockTicket")
                {
                    var ticket = x.Value.GetDataValue<Ticket>("Ticket");
                    if (ticket != null) ticket.Locked = false;
                    EventServiceFactory.EventService.PublishEvent(EventTopicNames.UnlockTicketRequested);
                }

                if (x.Value.Action.ActionType == "CloseActiveTicket")
                {
                    EventServiceFactory.EventService.PublishEvent(EventTopicNames.CloseTicketRequested, true);
                }

                if (x.Value.Action.ActionType == "UpdateResourceState")
                {
                    var resourceId = x.Value.GetDataValueAsInt("ResourceId");
                    var resourceTypeId = x.Value.GetDataValueAsInt("ResourceTypeId");
                    var stateName = x.Value.GetAsString("ResourceState");
                    var state = CacheService.GetResourceStateByName(stateName);
                    if (state != null)
                    {
                        if (resourceId > 0 && resourceTypeId > 0)
                        {
                            ResourceService.UpdateResourceState(resourceId, state.Id);
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
                                        ResourceService.UpdateResourceState(ticketResource.ResourceId, state.Id);
                                }
                            }
                        }
                    }
                }

                if (x.Value.Action.ActionType == "UpdateTicketAccount")
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
                            var account = Dao.Query(qFilter).FirstOrDefault();
                            if (account != null)
                                TicketService.UpdateResource(ticket, account);
                        }
                        else TicketService.UpdateResource(ticket, Resource.Null);
                    }
                }

                if (x.Value.Action.ActionType == "UpdateProgramSetting")
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

                if (x.Value.Action.ActionType == "RefreshCache")
                {
                    MethodQueue.Queue("ResetCache", ResetCache);
                }

                if (x.Value.Action.ActionType == "SendMessage")
                {
                    AppServices.MessagingService.SendMessage("ActionMessage", x.Value.GetAsString("Command"));
                }

                if (x.Value.Action.ActionType == "SendEmail")
                {
                    EMailService.SendEMailAsync(x.Value.GetAsString("SMTPServer"),
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

                if (x.Value.Action.ActionType == "UpdateTicketTax")
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

                if (x.Value.Action.ActionType == "UpdateTicketService")
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

                if (x.Value.Action.ActionType == "RegenerateTicketTax")
                {
                    var ticket = x.Value.GetDataValue<Ticket>("Ticket");
                    if (ticket != null)
                    {
                        TicketService.RegenerateTaxRates(ticket);
                        TicketService.RecalculateTicket(ticket);
                        EventServiceFactory.EventService.PublishEvent(EventTopicNames.RefreshSelectedTicket);
                    }
                }

                if (x.Value.Action.ActionType == "AddTicketDiscount")
                {
                    //var ticket = x.Value.GetDataValue<Ticket>("Ticket");
                    //if (ticket != null)
                    //{
                    //    var percentValue = x.Value.GetAsDecimal("DiscountPercentage");
                    //    ticket.AddTicketDiscount(DiscountType.Percent, percentValue, ApplicationState.CurrentLoggedInUser.Id);
                    //    TicketService.RecalculateTicket(ticket);
                    //}
                }

                if (x.Value.Action.ActionType == "AddOrder")
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

                if (x.Value.Action.ActionType == "UpdateTicketTag")
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

                if (x.Value.Action.ActionType == "TagOrder" || x.Value.Action.ActionType == "UntagOrder" || x.Value.Action.ActionType == "RemoveOrderTag")
                {
                    var order = x.Value.GetDataValue<Order>("Order");
                    if (order != null)
                    {
                        var tagName = x.Value.GetAsString("OrderTagName");
                        var orderTag = CacheService.GetOrderTagGroupByName(tagName);

                        if (orderTag != null)
                        {
                            if (x.Value.Action.ActionType == "RemoveOrderTag")
                            {
                                var tags = order.OrderTagValues.Where(y => y.OrderTagGroupId == orderTag.Id);
                                tags.ToList().ForEach(y => order.OrderTagValues.Remove(y));
                                return;
                            }
                            var tagValue = x.Value.GetAsString("OrderTagValue");
                            var orderTagValue = orderTag.OrderTags.SingleOrDefault(y => y.Name == tagValue);
                            if (orderTagValue != null)
                            {
                                if (x.Value.Action.ActionType == "TagOrder")
                                    order.TagIfNotTagged(orderTag, orderTagValue, ApplicationState.CurrentLoggedInUser.Id);
                                if (x.Value.Action.ActionType == "UntagOrder")
                                    order.UntagIfTagged(orderTag, orderTagValue);
                            }
                        }
                    }
                }

                if (x.Value.Action.ActionType == "MoveTaggedOrders")
                {
                    var ticket = x.Value.GetDataValue<Ticket>("Ticket");
                    var orderTagName = x.Value.GetAsString("OrderTagName");
                    if (ticket != null && !string.IsNullOrEmpty(orderTagName))
                    {
                        var orderTagValue = x.Value.GetAsString("OrderTagValue");
                        if (ticket.Orders.Any(y => y.OrderTagValues.Any(z => z.OrderTagGroupName == orderTagName && z.Name == orderTagValue)))
                        {
                            var tid = ticket.Id;
                            EventServiceFactory.EventService.PublishEvent(EventTopicNames.CloseTicketRequested, true);
                            ticket = TicketService.OpenTicket(tid);
                            var orders = ticket.Orders.Where(y => y.OrderTagValues.Any(z => z.OrderTagGroupName == orderTagName && z.Name == orderTagValue)).ToArray();
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

                if (x.Value.Action.ActionType == "UpdatePriceTag")
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

                if (x.Value.Action.ActionType == "ExecutePrintJob")
                {
                    var ticket = x.Value.GetDataValue<Ticket>("Ticket");
                    var pjName = x.Value.GetAsString("PrintJobName");
                    if (!string.IsNullOrEmpty(pjName))
                    {
                        TicketService.UpdateTicketNumber(ticket, ApplicationState.CurrentDepartment.TicketTemplate.TicketNumerator);
                        var j = CacheService.GetPrintJobByName(pjName);

                        if (j != null)
                        {
                            if (ticket != null)
                                PrinterService.PrintTicket(ticket, j);
                            else
                                PrinterService.ExecutePrintJob(j);
                        }
                    }
                }
            });
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
