using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Menus;
using Samba.Domain.Models.Tickets;

namespace Samba.Services
{
    public interface ICacheService
    {
        MenuItem GetMenuItem(Expression<Func<MenuItem, bool>> expression);
        IEnumerable<OrderTagGroup> GetOrderTagGroupsForItem(int menuItemId);
        IEnumerable<OrderTagGroup> GetOrderTagGroupsForItems(IEnumerable<int> menuItemIds);
        IEnumerable<MenuItemPortion> GetMenuItemPortions(int menuItemId);
        IEnumerable<string> GetTicketTagGroupNames();
        TicketTagGroup GetTicketTagGroupById(int id);
        AccountTransactionTemplate GetAccountTransactionTemplateById(int id);
        IEnumerable<Account> GetAccountsByTemplateId(int templateId);
        AccountTemplate GetAccountTemplateById(int accountTemplateId);
        Account GetAccountById(int accountId);
    }
}
