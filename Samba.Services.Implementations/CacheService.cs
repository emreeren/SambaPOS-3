using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Menus;
using Samba.Domain.Models.Resources;
using Samba.Domain.Models.Settings;
using Samba.Domain.Models.Tickets;
using Samba.Persistance.DaoClasses;

namespace Samba.Services.Implementations
{
    [Export(typeof(ICacheService))]
    class CacheService : ICacheService
    {
        private readonly ICacheDao _dataService;

        [ImportingConstructor]
        public CacheService(ICacheDao dataService)
        {
            _dataService = dataService;
        }

        private IEnumerable<MenuItem> _menuItems;
        public IEnumerable<MenuItem> MenuItems
        {
            get { return _menuItems ?? (_menuItems = _dataService.GetMenuItems()); }
        }

        public MenuItem GetMenuItem(Func<MenuItem, bool> expression)
        {
            return MenuItems.Single(expression);
        }

        private IEnumerable<ProductTimer> _productTimers;
        public IEnumerable<ProductTimer> ProductTimers
        {
            get { return _productTimers ?? (_productTimers = _dataService.GetProductTimers()); }
        }

        public ProductTimer GetProductTimer(int ticketTypeId, int terminalId, int departmentId, int userId, int menuItemId)
        {
            var tgl = ProductTimers.ToList();
            var mi = GetMenuItem(x => x.Id == menuItemId);
            var maps = tgl.SelectMany(x => x.ProductTimerMaps)
                .Where(x => x.TicketTypeId == 0 || x.TicketTypeId == ticketTypeId)
                .Where(x => x.TerminalId == 0 || x.TerminalId == terminalId)
                .Where(x => x.DepartmentId == 0 || x.DepartmentId == departmentId)
                .Where(x => x.UserRoleId == 0 || x.UserRoleId == userId)
                .Where(x => x.MenuItemGroupCode == null || x.MenuItemGroupCode == mi.GroupCode)
                .Where(x => x.MenuItemId == 0 || x.MenuItemId == menuItemId);
            return tgl.FirstOrDefault(x => maps.Any(y => y.ProductTimerId == x.Id));
        }

        private IEnumerable<OrderTagGroup> _orderTagGroups;
        public IEnumerable<OrderTagGroup> OrderTagGroups
        {
            get { return _orderTagGroups ?? (_orderTagGroups = _dataService.GetOrderTagGroups()); }
        }

        public IEnumerable<OrderTagGroup> InternalGetOrderTagGroups(int ticketTypeId, int terminalId, int departmentId, int userRoleId, int menuItemId)
        {
            var menuItem = GetMenuItem(x => x.Id == menuItemId);
            var tgl = OrderTagGroups.ToList();
            var maps = tgl.SelectMany(x => x.OrderTagMaps)
                .Where(x => x.TicketTypeId == 0 || x.TicketTypeId == ticketTypeId)
                .Where(x => x.TerminalId == 0 || x.TerminalId == terminalId)
                .Where(x => x.DepartmentId == 0 || x.DepartmentId == departmentId)
                .Where(x => x.UserRoleId == 0 || x.UserRoleId == userRoleId)
                .Where(x => x.MenuItemGroupCode == null || x.MenuItemGroupCode == menuItem.GroupCode)
                .Where(x => x.MenuItemId == 0 || x.MenuItemId == menuItemId);
            return tgl.Where(x => maps.Any(y => y.OrderTagGroupId == x.Id)).OrderBy(x => x.Order);
        }

        public IEnumerable<OrderTagGroup> GetOrderTagGroups(int ticketTypeId, int terminalId, int departmentId, int userRoleId, params int[] menuItemIds)
        {
            IEnumerable<OrderTagGroup> orderTags = OrderTagGroups.OrderBy(y => y.Order);
            return menuItemIds.Aggregate(orderTags, (x, y) => InternalGetOrderTagGroups(ticketTypeId, terminalId, departmentId, userRoleId, y));
        }

        public OrderTagGroup GetOrderTagGroupByName(string tagName)
        {
            return OrderTagGroups.FirstOrDefault(x => x.Name == tagName);
        }

        private IEnumerable<OrderStateGroup> _orderStateGroups;
        public IEnumerable<OrderStateGroup> OrderStateGroups
        {
            get { return _orderStateGroups ?? (_orderStateGroups = _dataService.GetOrderStateGroups()); }
        }

        public IEnumerable<OrderStateGroup> GetOrderStateGroups(int ticketTypeId, int terminalId, int departmentId, int userRoleId, params int[] menuItemIds)
        {
            IEnumerable<OrderStateGroup> orderStates = OrderStateGroups.OrderBy(y => y.Order);
            return menuItemIds.Aggregate(orderStates, (x, y) => InternalGetOrderStateGroups(ticketTypeId, terminalId, departmentId, userRoleId, y));
        }

        private IEnumerable<OrderStateGroup> InternalGetOrderStateGroups(int ticketTypeId, int terminalId, int departmentId, int userRoleId, int menuItemId)
        {
            var menuItem = GetMenuItem(x => x.Id == menuItemId);
            var tgl = OrderStateGroups.ToList();
            var maps = tgl.SelectMany(x => x.OrderStateMaps)
                .Where(x => x.TicketTypeId == 0 || x.TicketTypeId == ticketTypeId)
                .Where(x => x.TerminalId == 0 || x.TerminalId == terminalId)
                .Where(x => x.DepartmentId == 0 || x.DepartmentId == departmentId)
                .Where(x => x.UserRoleId == 0 || x.UserRoleId == userRoleId)
                .Where(x => x.MenuItemGroupCode == null || x.MenuItemGroupCode == menuItem.GroupCode)
                .Where(x => x.MenuItemId == 0 || x.MenuItemId == menuItemId);
            return tgl.Where(x => maps.Any(y => y.OrderStateGroupId == x.Id)).OrderBy(x => x.Order);
        }

        private IEnumerable<TicketTagGroup> _ticketTagGroups;
        public IEnumerable<TicketTagGroup> TicketTagGroups
        {
            get { return _ticketTagGroups ?? (_ticketTagGroups = _dataService.GetTicketTagGroups()); }
        }

        public IEnumerable<TicketTagGroup> GetTicketTagGroups(int ticketTypeId, int terminalId, int departmentId, int userRoleId)
        {
            var maps = TicketTagGroups.SelectMany(x => x.TicketTagMaps)
                .Where(x => x.TicketTypeId == 0 || x.TicketTypeId == ticketTypeId)
                .Where(x => x.TerminalId == 0 || x.TerminalId == terminalId)
                .Where(x => x.DepartmentId == 0 || x.DepartmentId == departmentId)
                .Where(x => x.UserRoleId == 0 || x.UserRoleId == userRoleId);
            return TicketTagGroups.Where(x => maps.Any(y => y.TicketTagGroupId == x.Id)).OrderBy(x => x.Order);
        }

        public IEnumerable<string> GetTicketTagGroupNames()
        {
            return TicketTagGroups.Select(x => x.Name).Distinct();
        }

        public TicketTagGroup GetTicketTagGroupById(int id)
        {
            return TicketTagGroups.FirstOrDefault(x => x.Id == id);
        }

        private IEnumerable<AccountTransactionDocumentType> _documentTypes;
        public IEnumerable<AccountTransactionDocumentType> DocumentTypes { get { return _documentTypes ?? (_documentTypes = _dataService.GetAccountTransactionDocumentTypes()); } }

        public IEnumerable<AccountTransactionDocumentType> GetAccountTransactionDocumentTypes(int accountTypeId, int terminalId, int userRoleId)
        {
            var maps = DocumentTypes.Where(x => x.MasterAccountTypeId == accountTypeId)
           .SelectMany(x => x.AccountTransactionDocumentTypeMaps)
           .Where(x => x.TerminalId == 0 || x.TerminalId == terminalId)
           .Where(x => x.UserRoleId == 0 || x.UserRoleId == userRoleId);
            return DocumentTypes.Where(x => maps.Any(y => y.AccountTransactionDocumentTypeId == x.Id)).OrderBy(x => x.Order);
        }

        public IEnumerable<AccountTransactionDocumentType> GetBatchDocumentTypes(IEnumerable<int> accountTypeIds, int terminalId, int userRoleId)
        {
            var maps = DocumentTypes.Where(x => x.BatchCreateDocuments && accountTypeIds.Contains(x.MasterAccountTypeId))
              .SelectMany(x => x.AccountTransactionDocumentTypeMaps)
              .Where(x => x.TerminalId == 0 || x.TerminalId == terminalId)
              .Where(x => x.UserRoleId == 0 || x.UserRoleId == userRoleId);
            return DocumentTypes.Where(x => maps.Any(y => y.AccountTransactionDocumentTypeId == x.Id)).OrderBy(x => x.Order);
        }

        public AccountTransactionDocumentType GetAccountTransactionDocumentTypeByName(string documentName)
        {
            return DocumentTypes.SingleOrDefault(x => x.Name == documentName);
        }

        private IEnumerable<PaymentType> _paymentTypes;
        public IEnumerable<PaymentType> PaymentTypes
        {
            get { return _paymentTypes ?? (_paymentTypes = _dataService.GetPaymentTypes()); }
        }

        public IEnumerable<PaymentType> GetUnderTicketPaymentTypes(int ticketTypeId, int terminalId, int departmentId, int userRoleId)
        {
            var maps = PaymentTypes.SelectMany(x => x.PaymentTypeMaps)
                .Where(x => x.DisplayUnderTicket)
                .Where(x => x.TicketTypeId == 0 || x.TicketTypeId == ticketTypeId)
                .Where(x => x.TerminalId == 0 || x.TerminalId == terminalId)
                .Where(x => x.DepartmentId == 0 || x.DepartmentId == departmentId)
                .Where(x => x.UserRoleId == 0 || x.UserRoleId == userRoleId);
            return PaymentTypes.Where(x => maps.Any(y => y.PaymentTypeId == x.Id)).OrderBy(x => x.Order);
        }

        public IEnumerable<PaymentType> GetPaymentScreenPaymentTypes(int ticketTypeId, int terminalId, int departmentId, int userRoleId)
        {
            var maps = PaymentTypes.SelectMany(x => x.PaymentTypeMaps)
                .Where(x => x.DisplayAtPaymentScreen)
                .Where(x => x.TicketTypeId == 0 || x.TicketTypeId == ticketTypeId)
                .Where(x => x.TerminalId == 0 || x.TerminalId == terminalId)
                .Where(x => x.DepartmentId == 0 || x.DepartmentId == departmentId)
                .Where(x => x.UserRoleId == 0 || x.UserRoleId == userRoleId);
            return PaymentTypes.Where(x => maps.Any(y => y.PaymentTypeId == x.Id)).OrderBy(x => x.Order);
        }

        public PaymentType GetPaymentTypeById(int paymentTypeId)
        {
            return PaymentTypes.Single(x => x.Id == paymentTypeId);
        }

        private IEnumerable<AccountTransactionType> _accountTransactionTypes;
        public IEnumerable<AccountTransactionType> AccountTransactionTypes
        {
            get { return _accountTransactionTypes ?? (_accountTransactionTypes = _dataService.GetAccountTransactionTypes()); }
        }

        public AccountTransactionType GetAccountTransactionTypeById(int id)
        {
            return AccountTransactionTypes.Single(x => x.Id == id);
        }

        public int GetAccountTransactionTypeIdByName(string accountTransactionTypeName)
        {
            return AccountTransactionTypes.Single(x => x.Name == accountTransactionTypeName).Id;
        }

        public AccountTransactionType FindAccountTransactionType(int sourceAccountTypeId, int targetAccountTypeId, int defaultSourceId, int defaultTargetId)
        {
            var result = AccountTransactionTypes.Where(
                x => x.SourceAccountTypeId == sourceAccountTypeId
                    && x.TargetAccountTypeId == targetAccountTypeId).ToList();

            if (defaultSourceId > 0 && result.Any(x => x.DefaultSourceAccountId == defaultSourceId))
                result = result.Where(x => x.DefaultSourceAccountId == defaultSourceId).ToList();

            if (defaultTargetId > 0 && result.Any(x => x.DefaultTargetAccountId == defaultTargetId))
                result = result.Where(x => x.DefaultTargetAccountId == defaultTargetId).ToList();

            return result.FirstOrDefault();
        }

        private IEnumerable<ResourceScreen> _resourceScreens;
        public IEnumerable<ResourceScreen> ResourceScreens
        {
            get { return _resourceScreens ?? (_resourceScreens = _dataService.GetResourceScreens()); }
        }

        public IEnumerable<ResourceScreen> GetResourceScreens(int terminalId, int departmentId, int userRoleId)
        {
            var maps = ResourceScreens.SelectMany(x => x.ResourceScreenMaps)
                .Where(x => x.TerminalId == 0 || x.TerminalId == terminalId)
                .Where(x => x.DepartmentId == 0 || x.DepartmentId == departmentId)
                .Where(x => x.UserRoleId == 0 || x.UserRoleId == userRoleId);
            return ResourceScreens.Where(x => maps.Any(y => y.ResourceScreenId == x.Id)).OrderBy(x => x.Order);
        }

        public IEnumerable<ResourceScreen> GetTicketResourceScreens(int ticketTypeId, int terminalId, int departmentId, int userRoleId)
        {
            var maps = ResourceScreens.SelectMany(x => x.ResourceScreenMaps)
                .Where(x => ticketTypeId == 0 || x.TicketTypeId == 0 || x.TicketTypeId == ticketTypeId)
                .Where(x => x.TerminalId == 0 || x.TerminalId == terminalId)
                .Where(x => x.DepartmentId == 0 || x.DepartmentId == departmentId)
                .Where(x => x.UserRoleId == 0 || x.UserRoleId == userRoleId);
            return ResourceScreens.Where(x => x.ResourceTypeId > 0 && maps.Any(y => y.ResourceScreenId == x.Id)).OrderBy(x => x.Order);
        }

        private IEnumerable<AccountScreen> _accountScreens;
        public IEnumerable<AccountScreen> AccountScreens
        {
            get { return _accountScreens ?? (_accountScreens = _dataService.GetAccountScreens()); }
        }

        public IEnumerable<AccountScreen> GetAccountScreens()
        {
            return AccountScreens;
        }

        private IEnumerable<ForeignCurrency> _foreignCurrencies;
        public IEnumerable<ForeignCurrency> ForeignCurrencies
        {
            get { return _foreignCurrencies ?? (_foreignCurrencies = _dataService.GetForeignCurrencies()); }
        }

        public IEnumerable<ForeignCurrency> GetForeignCurrencies()
        {
            return ForeignCurrencies;
        }

        public string GetCurrencySymbol(int currencyId)
        {
            return currencyId == 0 ? "" : GetForeignCurrencies().Single(x => x.Id == currencyId).CurrencySymbol;
        }

        public ForeignCurrency GetCurrencyById(int currencyId)
        {
            return GetForeignCurrencies().SingleOrDefault(x => x.Id == currencyId);
        }

        private IEnumerable<ScreenMenu> _screenMenus;
        public IEnumerable<ScreenMenu> ScreenMenus
        {
            get { return _screenMenus ?? (_screenMenus = _dataService.GetScreenMenus()); }
        }

        public ScreenMenu GetScreenMenu(int screenMenuId)
        {
            return ScreenMenus.Single(x => x.Id == screenMenuId);
        }

        public void ResetTicketTagCache()
        {
            _ticketTagGroups = null;
        }

        public void ResetOrderTagCache()
        {
            _orderTagGroups = null;
        }

        public void ResetCache()
        {
            _screenMenus = null;
            _foreignCurrencies = null;
            _accountScreens = null;
            _resourceScreens = null;
            _accountTransactionTypes = null;
            _paymentTypes = null;
            _documentTypes = null;
            _ticketTagGroups = null;
            _orderStateGroups = null;
            _orderTagGroups = null;
            _productTimers = null;
            _menuItems = null;
        }
    }
}
