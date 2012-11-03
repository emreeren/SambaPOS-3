using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Linq.Expressions;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Automation;
using Samba.Domain.Models.Menus;
using Samba.Domain.Models.Resources;
using Samba.Domain.Models.Settings;
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

        private IEnumerable<MenuItem> _menuItems;
        public IEnumerable<MenuItem> MenuItems
        {
            get { return _menuItems ?? (_menuItems = Dao.Query<MenuItem>(x => x.TaxTemplate.AccountTransactionType, x => x.Portions.Select(y => y.Prices))); }
        }

        public MenuItem GetMenuItem(Expression<Func<MenuItem, bool>> expression)
        {
            return MenuItems.Single(expression.Compile());
        }

        private IEnumerable<ProductTimer> _productTimers;
        public IEnumerable<ProductTimer> ProductTimers
        {
            get { return _productTimers ?? (_productTimers = Dao.Query<ProductTimer>(x => x.ProductTimerMaps)); }
        }

        private ProductTimer GetProductTimer(IEnumerable<ProductTimer> productTimers, int menuItemId)
        {
            var tgl = productTimers.ToList();
            var mi = GetMenuItem(x => x.Id == menuItemId);
            var maps = tgl.SelectMany(x => x.ProductTimerMaps)
                .Where(x => x.TerminalId == 0 || x.TerminalId == _applicationState.CurrentTerminal.Id)
                .Where(x => x.DepartmentId == 0 || x.DepartmentId == _applicationState.CurrentDepartment.Id)
                .Where(x => x.UserRoleId == 0 || x.UserRoleId == _applicationState.CurrentLoggedInUser.UserRole.Id)
                .Where(x => x.MenuItemGroupCode == null || x.MenuItemGroupCode == mi.GroupCode)
                .Where(x => x.MenuItemId == 0 || x.MenuItemId == mi.Id);
            return tgl.FirstOrDefault(x => maps.Any(y => y.ProductTimerId == x.Id));
        }

        public ProductTimer GetProductTimer(int menuItemId)
        {
            return GetProductTimer(ProductTimers, menuItemId);
        }

        public IEnumerable<OrderTagGroup> GetOrderTagGroupsForItem(int menuItemId)
        {
            return GetOrderTagGroups(OrderTagGroups, menuItemId);
        }

        public IEnumerable<OrderTagGroup> GetOrderTagGroupsForItems(IEnumerable<int> menuItemIds)
        {
            IEnumerable<OrderTagGroup> orderTags = OrderTagGroups.OrderBy(y => y.Order);
            return menuItemIds.Aggregate(orderTags, GetOrderTagGroups);
        }

        public OrderTagGroup GetOrderTagGroupByName(string tagName)
        {
            return OrderTagGroups.FirstOrDefault(x => x.Name == tagName);
        }

        public IEnumerable<MenuItemPortion> GetMenuItemPortions(int menuItemId)
        {
            return GetMenuItem(x => x.Id == menuItemId).Portions;
        }

        private IEnumerable<OrderTagGroup> _orderTagGroups;
        public IEnumerable<OrderTagGroup> OrderTagGroups
        {
            get { return _orderTagGroups ?? (_orderTagGroups = Dao.Query<OrderTagGroup>(x => x.OrderTags, x => x.OrderTagMaps)); }
        }

        private IEnumerable<OrderTagGroup> GetOrderTagGroups(IEnumerable<OrderTagGroup> tagGroups, int menuItemId)
        {
            var tgl = tagGroups.ToList();
            var mi = GetMenuItem(x => x.Id == menuItemId);
            var maps = tgl.SelectMany(x => x.OrderTagMaps)
                .Where(x => x.TerminalId == 0 || x.TerminalId == _applicationState.CurrentTerminal.Id)
                .Where(x => x.DepartmentId == 0 || x.DepartmentId == _applicationState.CurrentDepartment.Id)
                .Where(x => x.UserRoleId == 0 || x.UserRoleId == _applicationState.CurrentLoggedInUser.UserRole.Id)
                .Where(x => x.MenuItemGroupCode == null || x.MenuItemGroupCode == mi.GroupCode)
                .Where(x => x.MenuItemId == 0 || x.MenuItemId == mi.Id);
            return tgl.Where(x => maps.Any(y => y.OrderTagGroupId == x.Id)).OrderBy(x => x.Order);
        }

        private IEnumerable<OrderStateGroup> _orderStateGroups;
        public IEnumerable<OrderStateGroup> OrderStateGroups
        {
            get { return _orderStateGroups ?? (_orderStateGroups = Dao.Query<OrderStateGroup>(x => x.OrderStates, x => x.OrderStateMaps)); }
        }

        public IEnumerable<OrderStateGroup> GetOrderStateGroupsForItems(IEnumerable<int> menuItemIds)
        {
            IEnumerable<OrderStateGroup> orderStates = OrderStateGroups.OrderBy(y => y.Order);
            return menuItemIds.Aggregate(orderStates, GetOrderStateGroups);
        }

        private IEnumerable<OrderStateGroup> GetOrderStateGroups(IEnumerable<OrderStateGroup> stateGroups, int menuItemId)
        {
            var tgl = stateGroups.ToList();
            var mi = GetMenuItem(x => x.Id == menuItemId);
            var maps = tgl.SelectMany(x => x.OrderStateMaps)
                .Where(x => x.TerminalId == 0 || x.TerminalId == _applicationState.CurrentTerminal.Id)
                .Where(x => x.DepartmentId == 0 || x.DepartmentId == _applicationState.CurrentDepartment.Id)
                .Where(x => x.UserRoleId == 0 || x.UserRoleId == _applicationState.CurrentLoggedInUser.UserRole.Id)
                .Where(x => x.MenuItemGroupCode == null || x.MenuItemGroupCode == mi.GroupCode)
                .Where(x => x.MenuItemId == 0 || x.MenuItemId == mi.Id);
            return tgl.Where(x => maps.Any(y => y.OrderStateGroupId == x.Id)).OrderBy(x => x.Order);
        }

        public IEnumerable<string> GetTicketTagGroupNames()
        {
            return TicketTagGroups.Select(x => x.Name).Distinct();
        }

        public TicketTagGroup GetTicketTagGroupById(int id)
        {
            return TicketTagGroups.FirstOrDefault(x => x.Id == id);
        }

        private IEnumerable<AccountTransactionType> _accountTransactionTypes;
        public IEnumerable<AccountTransactionType> AccountTransactionTypes
        {
            get { return _accountTransactionTypes ?? (_accountTransactionTypes = Dao.Query<AccountTransactionType>()); }
        }

        public AccountTransactionType GetAccountTransactionTypeById(int id)
        {
            return AccountTransactionTypes.Single(x => x.Id == id);
        }

        public int GetAccountTransactionTypeIdByName(string accountTransactionTypeName)
        {
            return AccountTransactionTypes.Single(x => x.Name == accountTransactionTypeName).Id;
        }

        private IEnumerable<Resource> _resources;
        public IEnumerable<Resource> Resources
        {
            get { return _resources ?? (_resources = Dao.Query<Resource>()); }
        }

        public IEnumerable<Resource> GetResourcesByTemplateId(int templateId)
        {
            return Resources.Where(x => x.ResourceTypeId == templateId);
        }

        private IEnumerable<ResourceType> _resourceTypes;
        public IEnumerable<ResourceType> ResourceTypes
        {
            get { return _resourceTypes ?? (_resourceTypes = Dao.Query<ResourceType>(x => x.ResoruceCustomFields).OrderBy(x => x.Order)); }
        }

        private IEnumerable<AccountType> _accountTypes;
        public IEnumerable<AccountType> AccountTypes
        {
            get { return _accountTypes ?? (_accountTypes = Dao.Query<AccountType>()); }
        }

        public IEnumerable<ResourceType> GetResourceTypes()
        {
            return ResourceTypes;
        }

        public ResourceType GetResourceTypeById(int resourceTypeId)
        {
            return ResourceTypes.Single(x => x.Id == resourceTypeId);
        }

        public AccountType GetAccountTypeById(int accountTypeId)
        {
            return AccountTypes.Single(x => x.Id == accountTypeId);
        }

        public Account GetAccountById(int accountId)
        {
            return Dao.SingleWithCache<Account>(x => x.Id == accountId);
        }

        public Resource GetResourceById(int accountId)
        {
            return Dao.SingleWithCache<Resource>(x => x.Id == accountId);
        }

        private IEnumerable<AccountTransactionDocumentType> _documentTypes;
        public IEnumerable<AccountTransactionDocumentType> DocumentTypes { get { return _documentTypes ?? (_documentTypes = Dao.Query<AccountTransactionDocumentType>(x => x.TransactionTypes, x => x.AccountTransactionDocumentTypeMaps, x => x.AccountTransactionDocumentAccountMaps)); } }

        public IEnumerable<AccountTransactionDocumentType> GetAccountTransactionDocumentTypes(int accountTypeId)
        {
            var maps = DocumentTypes.Where(x => x.MasterAccountTypeId == accountTypeId)
               .SelectMany(x => x.AccountTransactionDocumentTypeMaps)
               .Where(x => x.TerminalId == 0 || x.TerminalId == _applicationState.CurrentTerminal.Id)
               .Where(x => x.UserRoleId == 0 || x.UserRoleId == _applicationState.CurrentLoggedInUser.UserRole.Id);
            return DocumentTypes.Where(x => maps.Any(y => y.AccountTransactionDocumentTypeId == x.Id)).OrderBy(x => x.Order);
        }

        public IEnumerable<AccountTransactionDocumentType> GetBatchDocumentTypes(IEnumerable<string> accountTypeNamesList)
        {
            var ids = GetAccountTypesByName(accountTypeNamesList).Select(x => x.Id);
            var maps = DocumentTypes.Where(x => x.BatchCreateDocuments && ids.Contains(x.MasterAccountTypeId))
               .SelectMany(x => x.AccountTransactionDocumentTypeMaps)
               .Where(x => x.TerminalId == 0 || x.TerminalId == _applicationState.CurrentTerminal.Id)
               .Where(x => x.UserRoleId == 0 || x.UserRoleId == _applicationState.CurrentLoggedInUser.UserRole.Id);
            return DocumentTypes.Where(x => maps.Any(y => y.AccountTransactionDocumentTypeId == x.Id)).OrderBy(x => x.Order);
        }

        public AccountTransactionDocumentType GetAccountTransactionDocumentTypeByName(string documentName)
        {
            return DocumentTypes.SingleOrDefault(x => x.Name == documentName);
        }

        private IEnumerable<ResourceState> _resourceStates;
        public IEnumerable<ResourceState> ResourceStates
        {
            get { return _resourceStates ?? (_resourceStates = Dao.Query<ResourceState>()); }
        }

        public ResourceState GetResourceStateById(int accountStateId)
        {
            return ResourceStates.SingleOrDefault(x => x.Id == accountStateId);
        }

        public ResourceState GetResourceStateByName(string stateName)
        {
            return ResourceStates.FirstOrDefault(x => x.Name == stateName);
        }

        public IEnumerable<ResourceState> GetResourceStates()
        {
            return ResourceStates;
        }

        private IEnumerable<PrintJob> _printJobs;
        public IEnumerable<PrintJob> PrintJobs
        {
            get { return _printJobs ?? (_printJobs = Dao.Query<PrintJob>(x => x.PrinterMaps)); }
        }

        public PrintJob GetPrintJobByName(string name)
        {
            return PrintJobs.SingleOrDefault(x => x.Name == name);
        }

        private IEnumerable<PaymentType> _paymentTypes;
        public IEnumerable<PaymentType> PaymentTypes
        {
            get { return _paymentTypes ?? (_paymentTypes = Dao.Query<PaymentType>(x => x.PaymentTypeMaps, x => x.AccountTransactionType, x => x.Account)); }
        }

        public IEnumerable<PaymentType> GetUnderTicketPaymentTypes()
        {
            var maps = PaymentTypes.SelectMany(x => x.PaymentTypeMaps)
                .Where(x => x.DisplayUnderTicket)
                .Where(x => x.TerminalId == 0 || x.TerminalId == _applicationState.CurrentTerminal.Id)
                .Where(x => x.DepartmentId == 0 || x.DepartmentId == _applicationState.CurrentDepartment.Id)
                .Where(x => x.UserRoleId == 0 || x.UserRoleId == _applicationState.CurrentLoggedInUser.UserRole.Id);
            return PaymentTypes.Where(x => maps.Any(y => y.PaymentTypeId == x.Id)).OrderBy(x => x.Order);
        }

        public IEnumerable<PaymentType> GetPaymentScreenPaymentTypes()
        {
            var maps = PaymentTypes.SelectMany(x => x.PaymentTypeMaps)
                .Where(x => x.DisplayAtPaymentScreen)
                .Where(x => x.TerminalId == 0 || x.TerminalId == _applicationState.CurrentTerminal.Id)
                .Where(x => x.DepartmentId == 0 || x.DepartmentId == _applicationState.CurrentDepartment.Id)
                .Where(x => x.UserRoleId == 0 || x.UserRoleId == _applicationState.CurrentLoggedInUser.UserRole.Id);
            return PaymentTypes.Where(x => maps.Any(y => y.PaymentTypeId == x.Id)).OrderBy(x => x.Order);
        }

        private IEnumerable<ChangePaymentType> _changePaymentTypes;
        public IEnumerable<ChangePaymentType> ChangePaymentTypes
        {
            get { return _changePaymentTypes ?? (_changePaymentTypes = Dao.Query<ChangePaymentType>(x => x.ChangePaymentTypeMaps, x => x.AccountTransactionType, x => x.Account)); }
        }

        public IEnumerable<ChangePaymentType> GetChangePaymentTypes()
        {
            var maps = ChangePaymentTypes.SelectMany(x => x.ChangePaymentTypeMaps)
                .Where(x => x.TerminalId == 0 || x.TerminalId == _applicationState.CurrentTerminal.Id)
                .Where(x => x.DepartmentId == 0 || x.DepartmentId == _applicationState.CurrentDepartment.Id)
                .Where(x => x.UserRoleId == 0 || x.UserRoleId == _applicationState.CurrentLoggedInUser.UserRole.Id);
            return ChangePaymentTypes.Where(x => maps.Any(y => y.ChangePaymentTypeId == x.Id)).OrderBy(x => x.Order);
        }

        private IEnumerable<TicketTagGroup> _ticketTagGroups;
        public IEnumerable<TicketTagGroup> TicketTagGroups
        {
            get { return _ticketTagGroups ?? (_ticketTagGroups = Dao.Query<TicketTagGroup>(x => x.TicketTags, x => x.TicketTagMaps)); }
        }

        public IEnumerable<TicketTagGroup> GetTicketTagGroups()
        {
            var maps = TicketTagGroups.SelectMany(x => x.TicketTagMaps)
                .Where(x => x.TerminalId == 0 || x.TerminalId == _applicationState.CurrentTerminal.Id)
                .Where(x => x.DepartmentId == 0 || x.DepartmentId == _applicationState.CurrentDepartment.Id)
                .Where(x => x.UserRoleId == 0 || x.UserRoleId == _applicationState.CurrentLoggedInUser.UserRole.Id);
            return TicketTagGroups.Where(x => maps.Any(y => y.TicketTagGroupId == x.Id)).OrderBy(x => x.Order);
        }

        private IEnumerable<AutomationCommand> _automationCommands;
        public IEnumerable<AutomationCommand> AutomationCommands
        {
            get { return _automationCommands ?? (_automationCommands = Dao.Query<AutomationCommand>(x => x.AutomationCommandMaps)); }
        }

        public IEnumerable<AutomationCommandData> GetAutomationCommands()
        {
            var currentDepartmentId = _applicationState.CurrentDepartment != null
                                          ? _applicationState.CurrentDepartment.Id
                                          : -1;
            var maps = AutomationCommands.SelectMany(x => x.AutomationCommandMaps)
                .Where(x => x.TerminalId == 0 || x.TerminalId == _applicationState.CurrentTerminal.Id)
                .Where(x => x.DepartmentId == 0 || x.DepartmentId == currentDepartmentId)
                .Where(x => x.UserRoleId == 0 || x.UserRoleId == _applicationState.CurrentLoggedInUser.UserRole.Id);
            var result = maps.Select(x => new AutomationCommandData { AutomationCommand = AutomationCommands.First(y => y.Id == x.AutomationCommandId), DisplayOnPayment = x.DisplayOnPayment, DisplayOnTicket = x.DisplayOnTicket, DisplayOnOrders = x.DisplayOnOrders, VisualBehaviour = x.VisualBehaviour });
            return result.OrderBy(x => x.AutomationCommand.Order);
        }

        private IEnumerable<CalculationSelector> _calculationSelectors;
        public IEnumerable<CalculationSelector> CalculationSelectors
        {
            get { return _calculationSelectors ?? (_calculationSelectors = Dao.Query<CalculationSelector>(x => x.CalculationSelectorMaps, x => x.CalculationTypes.Select(y => y.AccountTransactionType))); }
        }

        public IEnumerable<CalculationSelector> GetCalculationSelectors()
        {
            var maps = CalculationSelectors.SelectMany(x => x.CalculationSelectorMaps)
                .Where(x => x.TerminalId == 0 || x.TerminalId == _applicationState.CurrentTerminal.Id)
                .Where(x => x.DepartmentId == 0 || x.DepartmentId == _applicationState.CurrentDepartment.Id)
                .Where(x => x.UserRoleId == 0 || x.UserRoleId == _applicationState.CurrentLoggedInUser.UserRole.Id);
            return CalculationSelectors.Where(x => maps.Any(y => y.CalculationSelectorId == x.Id)).OrderBy(x => x.Order);
        }

        public IEnumerable<AccountType> GetAccountTypes()
        {
            return AccountTypes;
        }

        private IEnumerable<AccountScreen> _accountScreens;
        public IEnumerable<AccountScreen> AccountScreens
        {
            get { return _accountScreens ?? (_accountScreens = Dao.Query<AccountScreen>(x => x.AccountScreenValues)); }
        }

        public IEnumerable<AccountScreen> GetAccountScreens()
        {
            return AccountScreens;
        }

        public PaymentType GetPaymentTypeById(int paymentTypeId)
        {
            return PaymentTypes.Single(x => x.Id == paymentTypeId);
        }

        public ChangePaymentType GetChangePaymentTypeById(int id)
        {
            return ChangePaymentTypes.Single(x => x.Id == id);
        }

        public int GetResourceTypeIdByEntityName(string entityName)
        {
            var rt = ResourceTypes.FirstOrDefault(x => x.EntityName == entityName);
            return rt != null ? rt.Id : 0;
        }

        public IEnumerable<AccountType> GetAccountTypesByName(IEnumerable<string> accountTypeNames)
        {
            return AccountTypes.Where(x => accountTypeNames.Contains(x.Name));
        }

        public MenuItemPortion GetMenuItemPortion(int menuItemId, string portionName)
        {
            var mi = GetMenuItem(x => x.Id == menuItemId);
            if (mi.Portions.Count == 0) return null;
            return mi.Portions.FirstOrDefault(x => x.Name == portionName) ?? mi.Portions[0];
        }

        private IEnumerable<ScreenMenu> _screenMenus;
        public IEnumerable<ScreenMenu> ScreenMenus
        {
            get
            {
                return _screenMenus ?? (
                    _screenMenus = Dao.Query<ScreenMenu>(
                    x => x.Categories,
                    x => x.Categories.Select(z => z.ScreenMenuItems.Select(w => w.OrderTagTemplate.OrderTagTemplateValues.Select(x1 => x1.OrderTag))),
                    x => x.Categories.Select(z => z.ScreenMenuItems.Select(w => w.OrderTagTemplate.OrderTagTemplateValues.Select(x1 => x1.OrderTagGroup)))));
            }
        }

        public ScreenMenu GetScreenMenu(int screenMenuId)
        {
            return ScreenMenus.Single(x => x.Id == screenMenuId);
        }

        private IEnumerable<ResourceScreen> _resourceScreens;
        public IEnumerable<ResourceScreen> ResourceScreens
        {
            get { return _resourceScreens ?? (_resourceScreens = Dao.Query<ResourceScreen>(x => x.ResourceScreenMaps, x => x.ScreenItems, x => x.Widgets)); }
        }

        public IEnumerable<ResourceScreen> GetResourceScreens()
        {
            var maps = ResourceScreens.SelectMany(x => x.ResourceScreenMaps)
               .Where(x => x.TerminalId == 0 || x.TerminalId == _applicationState.CurrentTerminal.Id)
               .Where(x => x.DepartmentId == 0 || x.DepartmentId == _applicationState.CurrentDepartment.Id)
               .Where(x => x.UserRoleId == 0 || x.UserRoleId == _applicationState.CurrentLoggedInUser.UserRole.Id);
            return ResourceScreens.Where(x => maps.Any(y => y.ResourceScreenId == x.Id)).OrderBy(x => x.Order);
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

        public void ResetOrderTagCache()
        {
            _orderTagGroups = null;
        }

        public void ResetTicketTagCache()
        {
            _ticketTagGroups = null;
        }

        private IEnumerable<ForeignCurrency> _foreignCurrencies;
        public IEnumerable<ForeignCurrency> ForeignCurrencies
        {
            get { return _foreignCurrencies ?? (_foreignCurrencies = Dao.Query<ForeignCurrency>()); }
        }

        public IEnumerable<ForeignCurrency> GetForeignCurrencies()
        {
            return ForeignCurrencies;
        }

        public override void Reset()
        {
            _resourceScreens = null;
            _foreignCurrencies = null;
            _productTimers = null;
            _menuItems = null;
            _screenMenus = null;
            _accountTransactionTypes = null;
            _accountScreens = null;
            _calculationSelectors = null;
            _automationCommands = null;
            _orderTagGroups = null;
            _orderStateGroups = null;
            _resourceTypes = null;
            _accountTypes = null;
            _resources = null;
            _documentTypes = null;
            _resourceStates = null;
            _printJobs = null;
            _paymentTypes = null;
            _changePaymentTypes = null;
            _ticketTagGroups = null;
            Dao.ResetCache();
        }
    }
}
