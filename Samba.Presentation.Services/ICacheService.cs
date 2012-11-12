using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Menus;
using Samba.Domain.Models.Resources;
using Samba.Domain.Models.Settings;
using Samba.Domain.Models.Tickets;
using Samba.Presentation.Services.Common;

namespace Samba.Presentation.Services
{
    public interface ICacheService
    {
        void ResetOrderTagCache();
        void ResetTicketTagCache();

        ScreenMenu GetScreenMenu(int screenMenuId);
        MenuItem GetMenuItem(Expression<Func<MenuItem, bool>> expression);
        MenuItemPortion GetMenuItemPortion(int menuItemId, string portionName);
        ProductTimer GetProductTimer(int menuItemId);
        IEnumerable<OrderTagGroup> GetOrderTagGroupsForItem(int menuItemId);
        IEnumerable<OrderTagGroup> GetOrderTagGroupsForItems(IEnumerable<int> menuItemIds);
        IEnumerable<OrderStateGroup> GetOrderStateGroupsForItems(IEnumerable<int> menuItemIds);
        OrderTagGroup GetOrderTagGroupByName(string tagName);
        IEnumerable<MenuItemPortion> GetMenuItemPortions(int menuItemId);
        IEnumerable<string> GetTicketTagGroupNames();
        TicketTagGroup GetTicketTagGroupById(int id);
        AccountTransactionType GetAccountTransactionTypeById(int id);
        int GetAccountTransactionTypeIdByName(string accountTransactionTypeName);
        IEnumerable<Resource> GetResourcesByTemplateId(int templateId);
        IEnumerable<ResourceType> GetResourceTypes();
        ResourceType GetResourceTypeById(int resourceTypeId);
        int GetResourceTypeIdByEntityName(string entityName);
        Account GetAccountById(int accountId);
        Resource GetResourceById(int resourceId);
        IEnumerable<AccountTransactionDocumentType> GetAccountTransactionDocumentTypes(int accountTypeId);
        IEnumerable<AccountTransactionDocumentType> GetBatchDocumentTypes(IEnumerable<string> accountTypeNamesList);
        AccountTransactionDocumentType GetAccountTransactionDocumentTypeByName(string documentName);
        ResourceState GetResourceStateById(int resourceStateId);
        ResourceState GetResourceStateByName(string stateName);
        IEnumerable<ResourceState> GetResourceStates();
        PrintJob GetPrintJobByName(string name);
        IEnumerable<PaymentType> GetUnderTicketPaymentTypes();
        IEnumerable<PaymentType> GetPaymentScreenPaymentTypes();
        IEnumerable<ChangePaymentType> GetChangePaymentTypes();
        IEnumerable<TicketTagGroup> GetTicketTagGroups();
        IEnumerable<AutomationCommandData> GetAutomationCommands();
        IEnumerable<CalculationSelector> GetCalculationSelectors();
        AccountType GetAccountTypeById(int accountTypeId);
        IEnumerable<AccountType> GetAccountTypes();
        IEnumerable<AccountType> GetAccountTypesByName(IEnumerable<string> accountTypeNames);
        IEnumerable<AccountScreen> GetAccountScreens();
        PaymentType GetPaymentTypeById(int paymentTypeId);
        ChangePaymentType GetChangePaymentTypeById(int id);
        IEnumerable<ForeignCurrency> GetForeignCurrencies();
        IEnumerable<ResourceScreen> GetResourceScreens();
        IEnumerable<ResourceScreen> GetTicketResourceScreens();
        AccountTransactionType FindAccountTransactionType(int sourceAccountTypeId, int targetAccountTypeId, int defaultSourceId, int defaultTargetId);
        TicketType GetTicketTypeById(int ticketTypeId);
        IEnumerable<TicketType> GetTicketTypes();
        int GetTaskTypeIdByName(string taskTypeName);
        IEnumerable<string> GetTaskTypeNames();
    }
}
