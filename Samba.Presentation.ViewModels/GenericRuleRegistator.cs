using System;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows.Media;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Menus;
using Samba.Domain.Models.Settings;
using Samba.Domain.Models.Tickets;
using Samba.Domain.Models.Users;
using Samba.Localization.Properties;
using Samba.Persistance.Data;
using Samba.Presentation.Common;
using Samba.Presentation.Common.Services;
using Samba.Services;

namespace Samba.Presentation.ViewModels
{
    public static class GenericRuleRegistator
    {
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
            RuleActionTypeRegistry.RegisterActionType("SendEmail", Resources.SendEmail, new { SMTPServer = "", SMTPUser = "", SMTPPassword = "", SMTPPort = 0, ToEMailAddress = "", Subject = "", FromEMailAddress = "", EMailMessage = "", FileName = "", DeleteFile = false });
            RuleActionTypeRegistry.RegisterActionType("AddTicketDiscount", Resources.AddTicketDiscount, new { DiscountPercentage = 0m });
            RuleActionTypeRegistry.RegisterActionType("AddOrder", Resources.AddOrder, new { MenuItemName = "", PortionName = "", Quantity = 0, Tag = "" });
            RuleActionTypeRegistry.RegisterActionType("UpdateTicketTag", Resources.UpdateTicketTag, new { TagName = "", TagValue = "" });
            RuleActionTypeRegistry.RegisterActionType("TagOrder", "Tag Order", new { OrderTagName = "", OrderTagValue = "" });
            RuleActionTypeRegistry.RegisterActionType("UntagOrder", "Untag Order", new { OrderTagName = "", OrderTagValue = "" });
            RuleActionTypeRegistry.RegisterActionType("RemoveOrderTag", "Remove OrderTag", new { OrderTagName = "" });
            RuleActionTypeRegistry.RegisterActionType("UpdatePriceTag", Resources.UpdatePriceTag, new { DepartmentName = "", PriceTag = "" });
            RuleActionTypeRegistry.RegisterActionType("RefreshCache", Resources.RefreshCache);
            RuleActionTypeRegistry.RegisterActionType("SendMessage", Resources.BroadcastMessage, new { Command = "" });
            RuleActionTypeRegistry.RegisterActionType("UpdateProgramSetting", Resources.UpdateProgramSetting, new { SettingName = "", SettingValue = "" });
            RuleActionTypeRegistry.RegisterActionType("UpdateTicketTax", Resources.UpdateTicketTax, new { TaxTemplate = "" });
            RuleActionTypeRegistry.RegisterActionType("RegenerateTicketTax", Resources.RegenerateTicketTax);
            RuleActionTypeRegistry.RegisterActionType("UpdateTicketService", Resources.UpdateTicketService, new { ServiceTemplate = "", Amount = 0m });
            RuleActionTypeRegistry.RegisterActionType("UpdateTicketAccount", Resources.UpdateTicketAccount, new { AccountPhone = "", AccountName = "", Note = "" });
        }

        private static void RegisterRules()
        {
            RuleActionTypeRegistry.RegisterEvent(RuleEventNames.UserLoggedIn, Resources.UserLogin, new { UserName = "", RoleName = "" });
            RuleActionTypeRegistry.RegisterEvent(RuleEventNames.UserLoggedOut, Resources.UserLogout, new { UserName = "", RoleName = "" });
            RuleActionTypeRegistry.RegisterEvent(RuleEventNames.WorkPeriodStarts, Resources.WorkPeriodStarted, new { UserName = "" });
            RuleActionTypeRegistry.RegisterEvent(RuleEventNames.WorkPeriodEnds, Resources.WorkPeriodEnded, new { UserName = "" });
            RuleActionTypeRegistry.RegisterEvent(RuleEventNames.TriggerExecuted, Resources.TriggerExecuted, new { TriggerName = "" });
            RuleActionTypeRegistry.RegisterEvent(RuleEventNames.TicketCreated, Resources.TicketCreated);
            RuleActionTypeRegistry.RegisterEvent(RuleEventNames.TicketLocationChanged, Resources.TicketLocationChanged, new { OldLocation = "", NewLocation = "" });
            RuleActionTypeRegistry.RegisterEvent(RuleEventNames.TicketTagSelected, Resources.TicketTagSelected, new { TagName = "", TagValue = "", NumericValue = 0, TicketTag = "" });
            RuleActionTypeRegistry.RegisterEvent(RuleEventNames.OrderTagged, "Order Tagged", new { OrderTagName = "", OrderTagValue = "" });
            RuleActionTypeRegistry.RegisterEvent(RuleEventNames.OrderUntagged, "Order Untagged", new { OrderTagName = "", OrderTagValue = "" });
            RuleActionTypeRegistry.RegisterEvent(RuleEventNames.AccountSelectedForTicket, Resources.AccountSelectedForTicket, new { AccountName = "", PhoneNumber = "", AccountNote = "" });
            RuleActionTypeRegistry.RegisterEvent(RuleEventNames.TicketTotalChanged, Resources.TicketTotalChanged, new { TicketTotal = 0m, PreviousTotal = 0m, DiscountTotal = 0m, DiscountAmount = 0m, TipAmount = 0m });
            RuleActionTypeRegistry.RegisterEvent(RuleEventNames.MessageReceived, Resources.MessageReceived, new { Command = "" });
        }

        private static void RegisterParameterSources()
        {
            RuleActionTypeRegistry.RegisterParameterSoruce("UserName", () => AppServices.MainDataContext.Users.Select(x => x.Name));
            RuleActionTypeRegistry.RegisterParameterSoruce("DepartmentName", () => AppServices.MainDataContext.Departments.Select(x => x.Name));
            RuleActionTypeRegistry.RegisterParameterSoruce("TerminalName", () => AppServices.Terminals.Select(x => x.Name));
            RuleActionTypeRegistry.RegisterParameterSoruce("TriggerName", () => Dao.Select<Trigger, string>(yz => yz.Name, y => !string.IsNullOrEmpty(y.Expression)));
            RuleActionTypeRegistry.RegisterParameterSoruce("MenuItemName", () => Dao.Select<MenuItem, string>(yz => yz.Name, y => y.Id > 0));
            RuleActionTypeRegistry.RegisterParameterSoruce("PriceTag", () => Dao.Select<MenuItemPriceDefinition, string>(x => x.PriceTag, x => x.Id > 0));
            RuleActionTypeRegistry.RegisterParameterSoruce("Color", () => typeof(Colors).GetProperties(BindingFlags.Public | BindingFlags.Static).Select(x => x.Name));
            RuleActionTypeRegistry.RegisterParameterSoruce("TaxTemplate", () => Dao.Select<TaxTemplate, string>(x => x.Name, x => x.Id > 0));
            RuleActionTypeRegistry.RegisterParameterSoruce("ServiceTemplate", () => Dao.Select<ServiceTemplate, string>(x => x.Name, x => x.Id > 0));
            RuleActionTypeRegistry.RegisterParameterSoruce("TagName", () => Dao.Select<TicketTagGroup, string>(x => x.Name, x => x.Id > 0));
            RuleActionTypeRegistry.RegisterParameterSoruce("OrderTagName", () => Dao.Select<OrderTagGroup, string>(x => x.Name, x => x.Id > 0));
        }

        private static void ResetCache()
        {
            TriggerService.UpdateCronObjects();
            AppServices.ResetCache();
            AppServices.MainDataContext.SelectedDepartment.PublishEvent(EventTopicNames.SelectedDepartmentChanged);
        }

        private static void HandleEvents()
        {
            EventServiceFactory.EventService.GetEvent<GenericEvent<ActionData>>().Subscribe(x =>
            {
                if (x.Value.Action.ActionType == "UpdateTicketAccount")
                {
                    Expression<Func<Account, bool>> qFilter = null;

                    var phoneNumber = x.Value.GetAsString("AccountPhone");
                    var accountName = x.Value.GetAsString("AccountName");
                    var note = x.Value.GetAsString("Note");

                    if (!string.IsNullOrEmpty(phoneNumber))
                    {
                        qFilter = y => y.PhoneNumber == phoneNumber;
                    }

                    if (!string.IsNullOrEmpty(accountName))
                    {
                        if (qFilter == null) qFilter = y => y.Name == accountName;
                        else qFilter = qFilter.And(y => y.Name == accountName);
                    }

                    if (!string.IsNullOrEmpty(note))
                    {
                        if (qFilter == null) qFilter = y => y.Note == note;
                        else qFilter = qFilter.And(y => y.Note == note);
                    }

                    if (qFilter != null)
                    {
                        var account = Dao.Query(qFilter).FirstOrDefault();
                        if (account != null)
                            AppServices.MainDataContext.AssignAccountToSelectedTicket(account);
                    }
                    else AppServices.MainDataContext.AssignAccountToSelectedTicket(Account.Null);
                }

                if (x.Value.Action.ActionType == "UpdateProgramSetting")
                {
                    var settingName = x.Value.GetAsString("SettingName");
                    var settingValue = x.Value.GetAsString("SettingValue");
                    if (!string.IsNullOrEmpty(settingName))
                    {
                        AppServices.SettingService.GetCustomSetting(settingName).StringValue = settingValue;
                        AppServices.SettingService.SaveChanges();
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
                        x.Value.GetAsString("FromEMailAddress"),
                        x.Value.GetAsString("Subject"),
                        x.Value.GetAsString("EMailMessage"),
                        x.Value.GetAsString("FileName"),
                        x.Value.GetAsBoolean("DeleteFile"));
                }

                if (x.Value.Action.ActionType == "UpdateTicketTax")
                {
                    var ticket = x.Value.GetDataValue<Ticket>("Ticket");
                    if (ticket != null)
                    {
                        var taxTemplateName = x.Value.GetAsString("TaxTemplate");
                        var taxTemplate = AppServices.MainDataContext.TaxTemplates.FirstOrDefault(y => y.Name == taxTemplateName);
                        if (taxTemplate != null)
                        {
                            ticket.UpdateTax(taxTemplate);
                            TicketViewModel.RecalculateTicket(ticket);
                            EventServiceFactory.EventService.PublishEvent(EventTopicNames.RefreshSelectedTicket);
                        }
                    }
                }

                if (x.Value.Action.ActionType == "UpdateTicketService")
                {
                    var ticket = x.Value.GetDataValue<Ticket>("Ticket");
                    if (ticket != null)
                    {
                        var serviceTemplateName = x.Value.GetAsString("ServiceTemplate");
                        var serviceTemplate = AppServices.MainDataContext.ServiceTemplates.FirstOrDefault(
                                y => y.Name == serviceTemplateName);
                        if (serviceTemplate != null)
                        {
                            var amount = x.Value.GetAsDecimal("Amount");
                            ticket.AddService(serviceTemplate.Id, serviceTemplate.CalculationMethod, amount);
                            TicketViewModel.RecalculateTicket(ticket);
                        }
                    }
                }

                if (x.Value.Action.ActionType == "RegenerateTicketTax")
                {
                    var ticket = x.Value.GetDataValue<Ticket>("Ticket");
                    if (ticket != null)
                    {
                        TicketViewModel.RegenerateTaxRates(ticket);
                        TicketViewModel.RecalculateTicket(ticket);
                        EventServiceFactory.EventService.PublishEvent(EventTopicNames.RefreshSelectedTicket);
                    }
                }

                if (x.Value.Action.ActionType == "AddTicketDiscount")
                {
                    var ticket = x.Value.GetDataValue<Ticket>("Ticket");
                    if (ticket != null)
                    {
                        var percentValue = x.Value.GetAsDecimal("DiscountPercentage");
                        ticket.AddTicketDiscount(DiscountType.Percent, percentValue, AppServices.CurrentLoggedInUser.Id);
                        TicketViewModel.RecalculateTicket(ticket);
                    }
                }

                if (x.Value.Action.ActionType == "AddOrder")
                {
                    var ticket = x.Value.GetDataValue<Ticket>("Ticket");

                    if (ticket != null)
                    {
                        var menuItemName = x.Value.GetAsString("MenuItemName");
                        var menuItem = AppServices.DataAccessService.GetMenuItemByName(menuItemName);
                        var portionName = x.Value.GetAsString("PortionName");
                        var quantity = x.Value.GetAsDecimal("Quantity");
                        var tag = x.Value.GetAsString("Tag");

                        var ti = ticket.AddOrder(AppServices.CurrentLoggedInUser.Id, menuItem, portionName,
                                 AppServices.MainDataContext.SelectedDepartment.PriceTag);

                        ti.Quantity = quantity;
                        ti.Tag = tag;

                        TicketViewModel.RecalculateTicket(ticket);

                        EventServiceFactory.EventService.PublishEvent(EventTopicNames.RefreshSelectedTicket);
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
                        tagData.PublishEvent(EventTopicNames.TagSelectedForSelectedTicket);
                    }
                }

                if (x.Value.Action.ActionType == "TagOrder" || x.Value.Action.ActionType == "UntagOrder" || x.Value.Action.ActionType == "RemoveOrderTag")
                {
                    var order = x.Value.GetDataValue<Order>("Order");
                    if (order != null)
                    {
                        var tagName = x.Value.GetAsString("OrderTagName");
                        var orderTag = AppServices.MainDataContext.SelectedDepartment.OrderTagGroups.SingleOrDefault(y => y.Name == tagName);
                        if (x.Value.Action.ActionType == "RemoveOrderTag")
                        {
                            var tags = order.OrderTagValues.Where(y => y.OrderTagGroupId == orderTag.Id);
                            tags.ToList().ForEach(y => order.OrderTagValues.Remove(y));
                            return;
                        }
                        var tagValue = x.Value.GetAsString("OrderTagValue");
                        if (orderTag != null)
                        {
                            var orderTagValue = orderTag.OrderTags.SingleOrDefault(y => y.Name == tagValue);
                            if (orderTagValue != null)
                            {
                                if (x.Value.Action.ActionType == "TagOrder")
                                    order.TagIfNotTagged(orderTag, orderTagValue, AppServices.CurrentLoggedInUser.Id);
                                if (x.Value.Action.ActionType == "UntagOrder")
                                    order.UntagIfTagged(orderTag, orderTagValue, AppServices.CurrentLoggedInUser.Id);
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
            });
        }

        private static void RegisterNotifiers()
        {
            EventServiceFactory.EventService.GetEvent<GenericEvent<Message>>().Subscribe(x =>
            {
                if (x.Topic == EventTopicNames.MessageReceivedEvent && x.Value.Command == "ActionMessage")
                {
                    RuleExecutor.NotifyEvent(RuleEventNames.MessageReceived, new { Command = x.Value.Data });
                }
            });

            EventServiceFactory.EventService.GetEvent<GenericEvent<User>>().Subscribe(x =>
            {
                if (x.Topic == EventTopicNames.UserLoggedIn)
                {
                    RuleExecutor.NotifyEvent(RuleEventNames.UserLoggedIn, new { User = x.Value, UserName = x.Value.Name, RoleName = x.Value.UserRole.Name });
                }

                if (x.Topic == EventTopicNames.UserLoggedOut)
                {
                    RuleExecutor.NotifyEvent(RuleEventNames.UserLoggedOut, new { User = x.Value, UserName = x.Value.Name, RoleName = x.Value.UserRole.Name });
                }
            });
        }
    }
}
