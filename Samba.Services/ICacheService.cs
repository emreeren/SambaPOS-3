using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Menus;
using Samba.Domain.Models.Resources;
using Samba.Domain.Models.Settings;
using Samba.Domain.Models.Tickets;

namespace Samba.Services
{
    public interface ICacheService
    {
        ScreenMenu GetScreenMenu(int screenMenuId);
        MenuItem GetMenuItem(Expression<Func<MenuItem, bool>> expression);
        MenuItemPortion GetMenuItemPortion(int menuItemId, string portionName);
        IEnumerable<OrderTagGroup> GetOrderTagGroupsForItem(int menuItemId);
        IEnumerable<OrderTagGroup> GetOrderTagGroupsForItems(IEnumerable<int> menuItemIds);
        OrderTagGroup GetOrderTagGroupByName(string tagName);
        IEnumerable<MenuItemPortion> GetMenuItemPortions(int menuItemId);
        IEnumerable<string> GetTicketTagGroupNames();
        TicketTagGroup GetTicketTagGroupById(int id);
        AccountTransactionTemplate GetAccountTransactionTemplateById(int id);
        IEnumerable<Resource> GetResourcesByTemplateId(int templateId);
        IEnumerable<ResourceTemplate> GetResourceTemplates();
        ResourceTemplate GetResourceTemplateById(int resourceTemplateId);
        Account GetAccountById(int accountId);
        Resource GetResourceById(int resourceId);
        IEnumerable<AccountTransactionDocumentTemplate> GetAccountTransactionDocumentTemplates(int accountTemplateId);
        ResourceState GetResourceStateById(int resourceStateId);
        ResourceState GetResourceStateByName(string stateName);
        IEnumerable<ResourceState> GetResourceStates();
        PrintJob GetPrintJobByName(string name);
        IEnumerable<PaymentTemplate> GetUnderTicketPaymentTemplates();
        IEnumerable<PaymentTemplate> GetPaymentScreenPaymentTemplates();
        IEnumerable<TicketTagGroup> GetTicketTagGroups();
        IEnumerable<AutomationCommandData> GetAutomationCommands();
        IEnumerable<CalculationTemplate> GetCalculationTemplates();
        AccountTemplate GetAccountTemplateById(int accountTemplateId);
        IEnumerable<AccountTemplate> GetAccountTemplates();
        IEnumerable<AccountTemplate> GetAccountTemplatesByName(IEnumerable<string> accountTemplateNames);
        IEnumerable<AccountScreen> GetAccountScreens();
    }
}
