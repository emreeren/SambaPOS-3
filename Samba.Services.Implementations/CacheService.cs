using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Linq.Expressions;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Menus;
using Samba.Domain.Models.Tickets;
using Samba.Persistance.Data;
using Samba.Services.Common;

namespace Samba.Services.Implementations
{
    [Export(typeof(ICacheService))]
    class CacheService : AbstractService, ICacheService
    {
        private readonly IApplicationState _applicationState;

        [ImportingConstructor]
        public CacheService(IApplicationState applicationState)
        {
            _applicationState = applicationState;
        }

        public MenuItem GetMenuItem(Expression<Func<MenuItem, bool>> expression)
        {
            return Dao.SingleWithCache(expression, x => x.TaxTemplate, x => x.Portions.Select(y => y.Prices));
        }

        public IEnumerable<OrderTagGroup> GetOrderTagGroupsForItem(int menuItemId)
        {
            return GetOrderTagGroupsForItem(_applicationState.CurrentDepartment.TicketTemplate.OrderTagGroups, menuItemId);
        }

        public IEnumerable<OrderTagGroup> GetOrderTagGroupsForItems(IEnumerable<int> menuItemIds)
        {
            IEnumerable<OrderTagGroup> orderTags = _applicationState.CurrentDepartment.TicketTemplate.OrderTagGroups.OrderBy(y => y.Order);
            return menuItemIds.Aggregate(orderTags, GetOrderTagGroupsForItem);
        }

        public IEnumerable<MenuItemPortion> GetMenuItemPortions(int menuItemId)
        {
            return GetMenuItem(x => x.Id == menuItemId).Portions;
        }

        private IEnumerable<OrderTagGroup> GetOrderTagGroupsForItem(IEnumerable<OrderTagGroup> tagGroups, int menuItemId)
        {
            var mi = GetMenuItem(x => x.Id == menuItemId);

            var maps = tagGroups.SelectMany(x => x.OrderTagMaps)
                .Where(x => x.MenuItemGroupCode == mi.GroupCode || x.MenuItemGroupCode == null)
                .Where(x => x.MenuItemId == mi.Id || x.MenuItemId == 0);
            return tagGroups.Where(x => maps.Any(y => y.OrderTagGroupId == x.Id));
        }

        private IEnumerable<string> _ticketTagGroupNames;
        public IEnumerable<string> GetTicketTagGroupNames()
        {
            return _ticketTagGroupNames ?? (_ticketTagGroupNames = Dao.Distinct<TicketTagGroup>(x => x.Name));
        }

        public TicketTagGroup GetTicketTagGroupById(int id)
        {
            return Dao.SingleWithCache<TicketTagGroup>(x => x.Id == id, x => x.TicketTags);
        }

        public AccountTransactionTemplate GetAccountTransactionTemplateById(int id)
        {
            return Dao.SingleWithCache<AccountTransactionTemplate>(x => x.Id == id);
        }

        private IEnumerable<Account> _accounts;
        public IEnumerable<Account> Accounts
        {
            get { return _accounts ?? (_accounts = Dao.Query<Account>()); }
        }

        public IEnumerable<Account> GetAccountsByTemplateId(int templateId)
        {
            return Accounts.Where(x => x.AccountTemplateId == templateId);
        }

        private IEnumerable<AccountTemplate> _accountTemplates;
        public IEnumerable<AccountTemplate> AccountTemplates
        {
            get { return _accountTemplates ?? (_accountTemplates = Dao.Query<AccountTemplate>(x => x.AccountCustomFields)); }
        }

        public IEnumerable<AccountTemplate> GetAccountTemplates()
        {
            return AccountTemplates;
        }

        public AccountTemplate GetAccountTemplateById(int accountTemplateId)
        {
            return AccountTemplates.Single(x => x.Id == accountTemplateId);
        }

        public Account GetAccountById(int accountId)
        {
            return Dao.SingleWithCache<Account>(x => x.Id == accountId);
        }

        public override void Reset()
        {
            _ticketTagGroupNames = null;
            _accountTemplates = null;
            _accounts = null;
            Dao.ResetCache();
        }
    }
}
