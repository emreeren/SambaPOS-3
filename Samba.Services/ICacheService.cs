using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Menus;
using Samba.Domain.Models.Tickets;

namespace Samba.Services
{
    public interface ICacheService
    {
        void ResetCache();
        void ResetOrderTagCache();
        void ResetTicketTagCache();
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
        
    }
}
