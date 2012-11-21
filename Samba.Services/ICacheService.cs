using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Menus;
using Samba.Domain.Models.Resources;
using Samba.Domain.Models.Settings;
using Samba.Domain.Models.Tickets;

namespace Samba.Services
{
    public interface ICacheService
    {
        void ResetCache();
        void ResetOrderTagCache();
        void ResetTicketTagCache();
        ScreenMenu GetScreenMenu(int screenMenuId);
        MenuItem GetMenuItem(Func<MenuItem, bool> expression);
        ProductTimer GetProductTimer(int ticketTypeId, int terminalId, int departmentId, int userRoleId, int menuItemId);
        IEnumerable<OrderTagGroup> GetOrderTagGroups(int ticketTypeId, int terminalId, int departmentId, int userRoleId, params int[] menuItemIds);
        OrderTagGroup GetOrderTagGroupByName(string tagName);
        IEnumerable<OrderStateGroup> GetOrderStateGroups(int ticketTypeId, int terminalId, int departmentId, int userRoleId, params int[] menuItemIds);
        IEnumerable<TicketTagGroup> GetTicketTagGroups(int ticketTypeId, int terminalId, int departmentId, int userRoleId);
        IEnumerable<string> GetTicketTagGroupNames();
        TicketTagGroup GetTicketTagGroupById(int id);
        AccountTransactionDocumentType GetAccountTransactionDocumentTypeByName(string documentName);
        IEnumerable<AccountTransactionDocumentType> GetAccountTransactionDocumentTypes(int accountTypeId, int terminalId, int userRoleId);
        IEnumerable<AccountTransactionDocumentType> GetBatchDocumentTypes(IEnumerable<int> accountTypeIds, int terminalId, int userRoleId);
        IEnumerable<PaymentType> GetUnderTicketPaymentTypes(int ticketTypeId, int terminalId, int departmentId, int userRoleId);
        IEnumerable<PaymentType> GetPaymentScreenPaymentTypes(int ticketTypeId, int terminalId, int departmentId, int userRoleId);
        PaymentType GetPaymentTypeById(int paymentTypeId);
        AccountTransactionType FindAccountTransactionType(int sourceAccountTypeId, int targetAccountTypeId, int defaultSourceId, int defaultTargetId);
        AccountTransactionType GetAccountTransactionTypeById(int id);
        int GetAccountTransactionTypeIdByName(string accountTransactionTypeName);
        IEnumerable<ResourceScreen> GetResourceScreens(int terminalId, int departmentId, int userRoleId);
        IEnumerable<ResourceScreen> GetTicketResourceScreens(int ticketTypeId, int terminalId, int departmentId, int userRoleId);
        IEnumerable<AccountScreen> GetAccountScreens();
        IEnumerable<ForeignCurrency> GetForeignCurrencies();
        string GetCurrencySymbol(int currencyId);
        ForeignCurrency GetCurrencyById(int currencyId);
    }
}
