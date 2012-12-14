using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Menus;
using Samba.Domain.Models.Resources;
using Samba.Domain.Models.Settings;
using Samba.Domain.Models.Tickets;
using Samba.Services.Common;

namespace Samba.Services
{
    public interface ICacheService
    {
        void ResetCache();
        void ResetOrderTagCache();
        void ResetTicketTagCache();
        Account GetAccountById(int accountId);
        Resource GetResourceById(int resourceId);
        ScreenMenu GetScreenMenu(int screenMenuId);
        MenuItem GetMenuItem(Func<MenuItem, bool> expression);
        string GetMenuItemData(int menuItemId, Func<MenuItem, string> selector);
        MenuItemPortion GetMenuItemPortion(int menuItemId, string portionName);
        IEnumerable<MenuItemPortion> GetMenuItemPortions(int menuItemId);
        ProductTimer GetProductTimer(int ticketTypeId, int terminalId, int departmentId, int userRoleId, int menuItemId);
        IEnumerable<OrderTagGroup> GetOrderTagGroups(int ticketTypeId, int terminalId, int departmentId, int userRoleId, params int[] menuItemIds);
        IEnumerable<TaxTemplate> GetTaxTemplates(int ticketTypeId, int terminalId, int departmentId, int userRoleId, int menuItemId);
        OrderTagGroup GetOrderTagGroupByName(string tagName);
        IEnumerable<TicketTagGroup> GetTicketTagGroups(int ticketTypeId, int terminalId, int departmentId, int userRoleId);
        IEnumerable<string> GetTicketTagGroupNames();
        TicketTagGroup GetTicketTagGroupById(int id);
        AccountTransactionDocumentType GetAccountTransactionDocumentTypeByName(string documentName);
        IEnumerable<AccountTransactionDocumentType> GetAccountTransactionDocumentTypes(int accountTypeId, int terminalId, int userRoleId);
        IEnumerable<AccountTransactionDocumentType> GetBatchDocumentTypes(IEnumerable<string> accountTypeNames, int terminalId, int userRoleId);
        IEnumerable<PaymentType> GetUnderTicketPaymentTypes(int ticketTypeId, int terminalId, int departmentId, int userRoleId);
        IEnumerable<PaymentType> GetPaymentScreenPaymentTypes(int ticketTypeId, int terminalId, int departmentId, int userRoleId);
        PaymentType GetPaymentTypeById(int paymentTypeId);
        AccountTransactionType FindAccountTransactionType(int sourceAccountTypeId, int targetAccountTypeId, int defaultSourceId, int defaultTargetId);
        AccountTransactionType GetAccountTransactionTypeById(int id);
        AccountTransactionType GetAccountTransactionTypeByName(string name);
        int GetAccountTransactionTypeIdByName(string accountTransactionTypeName);
        IEnumerable<ResourceScreen> GetResourceScreens(int terminalId, int departmentId, int userRoleId);
        IEnumerable<ResourceScreen> GetTicketResourceScreens(int ticketTypeId, int terminalId, int departmentId, int userRoleId);
        IEnumerable<AccountScreen> GetAccountScreens();
        IEnumerable<ForeignCurrency> GetForeignCurrencies();
        string GetCurrencySymbol(int currencyId);
        ForeignCurrency GetCurrencyById(int currencyId);
        int GetTaskTypeIdByName(string taskTypeName);
        IEnumerable<string> GetTaskTypeNames();
        TicketType GetTicketTypeById(int ticketTypeId);
        IEnumerable<TicketType> GetTicketTypes();
        IEnumerable<CalculationSelector> GetCalculationSelectors(int ticketTypeId, int terminalId, int departmentId, int userRoleId);
        IEnumerable<AutomationCommandData> GetAutomationCommands(int ticketTypeId, int terminalId, int departmentId, int userRoleId);
        IEnumerable<ChangePaymentType> GetChangePaymentTypes(int ticketTypeId, int terminalId, int departmentId, int userRoleId);
        ChangePaymentType GetChangePaymentTypeById(int id);
        AccountType GetAccountTypeById(int accountTypeId);
        IEnumerable<AccountType> GetAccountTypes();
        IEnumerable<AccountType> GetAccountTypesByName(IEnumerable<string> accountTypeNames);
        PrintJob GetPrintJobByName(string name);
        IEnumerable<ResourceType> GetResourceTypes();
        ResourceType GetResourceTypeById(int resourceTypeId);
        int GetResourceTypeIdByEntityName(string entityName);
        IEnumerable<State> GetStates(int stateType);
        string GetStateColor(string resourceState);
        IEnumerable<Resource> GetResources(int resourceTypeId,string stateData);
        Resource GetResourceByName(string resourceTypeName, string resourceName);
        
    }
}
