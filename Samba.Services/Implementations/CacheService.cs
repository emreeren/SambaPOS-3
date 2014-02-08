using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Automation;
using Samba.Domain.Models.Entities;
using Samba.Domain.Models.Inventory;
using Samba.Domain.Models.Menus;
using Samba.Domain.Models.Settings;
using Samba.Domain.Models.Tasks;
using Samba.Domain.Models.Tickets;
using Samba.Persistance;
using Samba.Persistance.Data;
using Samba.Services.Common;

namespace Samba.Services.Implementations
{
    [Export(typeof(ICacheService))]
    class CacheService : ICacheService
    {
        private readonly ICacheDao _cacheDao;
        private readonly IPrinterDao _printerDao;
        private readonly EntityCache _entityCache;

        [ImportingConstructor]
        public CacheService(ICacheDao cacheDao, IPrinterDao printerDao)
        {
            _cacheDao = cacheDao;
            _printerDao = printerDao;
            _entityCache = new EntityCache();
        }

        private IEnumerable<AppRule> _rules;
        public IEnumerable<AppRule> Rules { get { return _rules ?? (_rules = _cacheDao.GetRules()); } }

        public IEnumerable<AppRule> GetAppRules(string eventName, int terminalId, int departmentId, int userRoleId, int ticketTypeId)
        {
            var maps = Rules.Where(x => x.EventName == eventName).SelectMany(x => x.AppRuleMaps)
                .Where(x => x.TerminalId == 0 || x.TerminalId == terminalId)
                .Where(x => x.DepartmentId == 0 || x.DepartmentId == departmentId)
                .Where(x => x.UserRoleId == 0 || x.UserRoleId == userRoleId)
                .Where(x => x.TicketTypeId == 0 || x.TicketTypeId == ticketTypeId);
            return Rules.Where(x => maps.Any(y => y.AppRuleId == x.Id)).OrderBy(x => x.SortOrder);
        }

        private IEnumerable<AppAction> _actions;
        public IEnumerable<AppAction> Actions { get { return _actions ?? (_actions = _cacheDao.GetActions()); } }

        public IEnumerable<AppAction> GetActions()
        {
            return Actions;
        }

        public Account GetAccountById(int accountId)
        {
            return Dao.SingleWithCache<Account>(x => x.Id == accountId);
        }

        public Entity GetEntityById(int accountId)
        {
            return Dao.SingleWithCache<Entity>(x => x.Id == accountId);
        }

        private IEnumerable<MenuItem> _menuItems;
        public IEnumerable<MenuItem> MenuItems
        {
            get { return _menuItems ?? (_menuItems = _cacheDao.GetMenuItems()); }
        }

        public MenuItem GetMenuItem(Func<MenuItem, bool> expression)
        {
            return MenuItems.Single(expression);
        }

        public MenuItem FindMenuItemByBarcode(string insertedData)
        {
            if (string.IsNullOrWhiteSpace(insertedData)) return null;
            return MenuItems.SingleOrDefault(x => x.Barcode == insertedData);
        }

        public string GetMenuItemData(int menuItemId, Func<MenuItem, string> selector)
        {
            return MenuItems.Where(x => x.Id == menuItemId).Select(selector).FirstOrDefault();
        }

        public IEnumerable<MenuItemPortion> GetMenuItemPortions(int menuItemId)
        {
            return GetMenuItem(x => x.Id == menuItemId).Portions;
        }

        public MenuItemPortion GetMenuItemPortion(int menuItemId, string portionName)
        {
            var mi = GetMenuItem(x => x.Id == menuItemId);
            if (mi.Portions.Count == 0) return null;
            return mi.Portions.FirstOrDefault(x => x.Name == portionName) ?? mi.Portions[0];
        }

        private IEnumerable<ProductTimer> _productTimers;
        public IEnumerable<ProductTimer> ProductTimers
        {
            get { return _productTimers ?? (_productTimers = _cacheDao.GetProductTimers()); }
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
            get { return _orderTagGroups ?? (_orderTagGroups = _cacheDao.GetOrderTagGroups()); }
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
            return tgl.Where(x => maps.Any(y => y.OrderTagGroupId == x.Id)).OrderBy(x => x.SortOrder);
        }

        public IEnumerable<OrderTagGroup> GetOrderTagGroups(int ticketTypeId, int terminalId, int departmentId, int userRoleId, params int[] menuItemIds)
        {
            IEnumerable<OrderTagGroup> orderTags = OrderTagGroups.OrderBy(y => y.SortOrder);
            return menuItemIds.Aggregate(orderTags, (x, y) => InternalGetOrderTagGroups(ticketTypeId, terminalId, departmentId, userRoleId, y));
        }

        public OrderTagGroup GetOrderTagGroupByName(string tagName)
        {
            return OrderTagGroups.FirstOrDefault(x => x.Name == tagName);
        }

        public OrderTagGroup GetOrderTagGroupByOrderTagName(string orderTag)
        {
            return OrderTagGroups.FirstOrDefault(x => x.OrderTags.Any(y => y.Name == orderTag));
        }

        private IEnumerable<TaxTemplate> _taxTemplates;
        public IEnumerable<TaxTemplate> TaxTemplates
        {
            get { return _taxTemplates ?? (_taxTemplates = _cacheDao.GetTaxTemplates()); }
        }

        public IEnumerable<TaxTemplate> GetTaxTemplates(int ticketTypeId, int terminalId, int departmentId, int userRoleId, int menuItemId)
        {
            var menuItem = GetMenuItem(x => x.Id == menuItemId);
            var tgl = TaxTemplates.ToList();
            var maps = tgl.SelectMany(x => x.TaxTemplateMaps)
                .Where(x => x.TicketTypeId == 0 || x.TicketTypeId == ticketTypeId)
                .Where(x => x.TerminalId == 0 || x.TerminalId == terminalId)
                .Where(x => x.DepartmentId == 0 || x.DepartmentId == departmentId)
                .Where(x => x.UserRoleId == 0 || x.UserRoleId == userRoleId)
                .Where(x => x.MenuItemGroupCode == null || x.MenuItemGroupCode == menuItem.GroupCode)
                .Where(x => x.MenuItemId == 0 || x.MenuItemId == menuItemId);
            return tgl.Where(x => maps.Any(y => y.TaxTemplateId == x.Id)).OrderBy(x => x.SortOrder);
        }

        private IEnumerable<TicketTagGroup> _ticketTagGroups;
        public IEnumerable<TicketTagGroup> TicketTagGroups
        {
            get { return _ticketTagGroups ?? (_ticketTagGroups = _cacheDao.GetTicketTagGroups()); }
        }

        public IEnumerable<TicketTagGroup> GetTicketTagGroups(int ticketTypeId, int terminalId, int departmentId, int userRoleId)
        {
            var maps = TicketTagGroups.SelectMany(x => x.TicketTagMaps)
                .Where(x => x.TicketTypeId == 0 || x.TicketTypeId == ticketTypeId)
                .Where(x => x.TerminalId == 0 || x.TerminalId == terminalId)
                .Where(x => x.DepartmentId == 0 || x.DepartmentId == departmentId)
                .Where(x => x.UserRoleId == 0 || x.UserRoleId == userRoleId);
            return TicketTagGroups.Where(x => maps.Any(y => y.TicketTagGroupId == x.Id)).OrderBy(x => x.SortOrder);
        }

        public IEnumerable<string> GetTicketTagGroupNames()
        {
            return TicketTagGroups.Select(x => x.Name).Distinct();
        }

        public TicketTagGroup GetTicketTagGroupById(int id)
        {
            return TicketTagGroups.FirstOrDefault(x => x.Id == id);
        }

        public TicketTagGroup GetTicketTagGroupByName(string name)
        {
            return TicketTagGroups.FirstOrDefault(x => x.Name == name);
        }

        private IEnumerable<AccountTransactionDocumentType> _documentTypes;
        public IEnumerable<AccountTransactionDocumentType> DocumentTypes { get { return _documentTypes ?? (_documentTypes = _cacheDao.GetAccountTransactionDocumentTypes()); } }

        public IEnumerable<AccountTransactionDocumentType> GetAccountTransactionDocumentTypes(int accountTypeId, int terminalId, int userRoleId)
        {
            var maps = DocumentTypes.Where(x => x.MasterAccountTypeId == accountTypeId)
           .SelectMany(x => x.AccountTransactionDocumentTypeMaps)
           .Where(x => x.TerminalId == 0 || x.TerminalId == terminalId)
           .Where(x => x.UserRoleId == 0 || x.UserRoleId == userRoleId);
            return DocumentTypes.Where(x => maps.Any(y => y.AccountTransactionDocumentTypeId == x.Id)).OrderBy(x => x.SortOrder);
        }

        public IEnumerable<AccountTransactionDocumentType> GetBatchDocumentTypes(IEnumerable<int> accountTypeIds, int terminalId, int userRoleId)
        {
            var maps = DocumentTypes.Where(x => x.BatchCreateDocuments && accountTypeIds.Contains(x.MasterAccountTypeId))
              .SelectMany(x => x.AccountTransactionDocumentTypeMaps)
              .Where(x => x.TerminalId == 0 || x.TerminalId == terminalId)
              .Where(x => x.UserRoleId == 0 || x.UserRoleId == userRoleId);
            return DocumentTypes.Where(x => maps.Any(y => y.AccountTransactionDocumentTypeId == x.Id)).OrderBy(x => x.SortOrder);
        }

        public IEnumerable<AccountTransactionDocumentType> GetBatchDocumentTypes(IEnumerable<string> accountTypeNamesList, int terminalId, int userRoleId)
        {
            var ids = GetAccountTypesByName(accountTypeNamesList).Select(x => x.Id);
            return GetBatchDocumentTypes(ids, terminalId, userRoleId);
        }

        public AccountTransactionDocumentType GetAccountTransactionDocumentTypeByName(string documentName)
        {
            return DocumentTypes.SingleOrDefault(x => x.Name == documentName);
        }

        public AccountTransactionDocumentType GetAccountTransactionDocumentTypeById(int documentId)
        {
            return DocumentTypes.SingleOrDefault(x => x.Id == documentId);
        }

        private IEnumerable<ChangePaymentType> _changePaymentTypes;
        public IEnumerable<ChangePaymentType> ChangePaymentTypes
        {
            get { return _changePaymentTypes ?? (_changePaymentTypes = _cacheDao.GetChangePaymentTypes()); }
        }

        public IEnumerable<ChangePaymentType> GetChangePaymentTypes(int ticketTypeId, int terminalId, int departmentId, int userRoleId)
        {
            var maps = ChangePaymentTypes.SelectMany(x => x.ChangePaymentTypeMaps)
                .Where(x => x.TicketTypeId == 0 || x.TicketTypeId == ticketTypeId)
                .Where(x => x.TerminalId == 0 || x.TerminalId == terminalId)
                .Where(x => x.DepartmentId == 0 || x.DepartmentId == departmentId)
                .Where(x => x.UserRoleId == 0 || x.UserRoleId == userRoleId);
            return ChangePaymentTypes.Where(x => maps.Any(y => y.ChangePaymentTypeId == x.Id)).OrderBy(x => x.SortOrder);
        }

        public ChangePaymentType GetChangePaymentTypeById(int id)
        {
            return ChangePaymentTypes.Single(x => x.Id == id);
        }

        private IEnumerable<PaymentType> _paymentTypes;
        public IEnumerable<PaymentType> PaymentTypes
        {
            get { return _paymentTypes ?? (_paymentTypes = _cacheDao.GetPaymentTypes()); }
        }

        public IEnumerable<PaymentType> GetPaymentScreenPaymentTypes(int ticketTypeId, int terminalId, int departmentId, int userRoleId)
        {
            var maps = PaymentTypes.SelectMany(x => x.PaymentTypeMaps)
                .Where(x => x.TicketTypeId == 0 || x.TicketTypeId == ticketTypeId)
                .Where(x => x.TerminalId == 0 || x.TerminalId == terminalId)
                .Where(x => x.DepartmentId == 0 || x.DepartmentId == departmentId)
                .Where(x => x.UserRoleId == 0 || x.UserRoleId == userRoleId);
            return PaymentTypes.Where(x => maps.Any(y => y.PaymentTypeId == x.Id)).OrderBy(x => x.SortOrder);
        }

        public PaymentType GetPaymentTypeById(int paymentTypeId)
        {
            return PaymentTypes.Single(x => x.Id == paymentTypeId);
        }

        private IEnumerable<AccountTransactionType> _accountTransactionTypes;
        public IEnumerable<AccountTransactionType> AccountTransactionTypes
        {
            get { return _accountTransactionTypes ?? (_accountTransactionTypes = _cacheDao.GetAccountTransactionTypes()); }
        }

        public AccountTransactionType GetAccountTransactionTypeById(int id)
        {
            return AccountTransactionTypes.Single(x => x.Id == id);
        }

        public AccountTransactionType GetAccountTransactionTypeByName(string name)
        {
            return AccountTransactionTypes.SingleOrDefault(x => x.Name == name);
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

        private IEnumerable<EntityScreen> _entityScreens;
        public IEnumerable<EntityScreen> EntityScreens
        {
            get { return _entityScreens ?? (_entityScreens = _cacheDao.GetEntityScreens()); }
        }

        public IEnumerable<EntityScreen> GetEntityScreens(int terminalId, int departmentId, int userRoleId)
        {
            var maps = EntityScreens.SelectMany(x => x.EntityScreenMaps)
                .Where(x => x.TerminalId == 0 || x.TerminalId == terminalId)
                .Where(x => x.DepartmentId == 0 || x.DepartmentId == departmentId)
                .Where(x => x.UserRoleId == 0 || x.UserRoleId == userRoleId);
            return EntityScreens.Where(x => maps.Any(y => y.EntityScreenId == x.Id)).OrderBy(x => x.SortOrder);
        }

        public IEnumerable<EntityScreen> GetTicketEntityScreens(int ticketTypeId, int terminalId, int departmentId, int userRoleId)
        {
            var maps = EntityScreens.SelectMany(x => x.EntityScreenMaps)
                .Where(x => ticketTypeId == 0 || x.TicketTypeId == 0 || x.TicketTypeId == ticketTypeId)
                .Where(x => x.TerminalId == 0 || x.TerminalId == terminalId)
                .Where(x => x.DepartmentId == 0 || x.DepartmentId == departmentId)
                .Where(x => x.UserRoleId == 0 || x.UserRoleId == userRoleId);
            return EntityScreens.Where(x => x.EntityTypeId > 0 && maps.Any(y => y.EntityScreenId == x.Id)).OrderBy(x => x.SortOrder);
        }

        private IEnumerable<AccountScreen> _accountScreens;
        public IEnumerable<AccountScreen> AccountScreens
        {
            get { return _accountScreens ?? (_accountScreens = _cacheDao.GetAccountScreens()); }
        }

        public IEnumerable<AccountScreen> GetAccountScreens()
        {
            return AccountScreens;
        }

        private IEnumerable<ForeignCurrency> _foreignCurrencies;
        public IEnumerable<ForeignCurrency> ForeignCurrencies
        {
            get { return _foreignCurrencies ?? (_foreignCurrencies = _cacheDao.GetForeignCurrencies()); }
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
            get { return _screenMenus ?? (_screenMenus = _cacheDao.GetScreenMenus()); }
        }

        public ScreenMenu GetScreenMenu(int screenMenuId)
        {
            return ScreenMenus.Single(x => x.Id == screenMenuId);
        }

        private IEnumerable<TaskType> _taskTypes;
        public IEnumerable<TaskType> TaskTypes
        {
            get { return _taskTypes ?? (_taskTypes = _cacheDao.GetTaskTypes()); }
        }

        public int GetTaskTypeIdByName(string taskTypeName)
        {
            var taskType = TaskTypes.FirstOrDefault(x => x.Name == taskTypeName);
            return taskType != null ? taskType.Id : 0;
        }

        public TaskType GetTaskTypeByName(string taskTypeName)
        {
            return TaskTypes.FirstOrDefault(x => x.Name == taskTypeName);
        }

        public IEnumerable<string> GetTaskTypeNames()
        {
            return TaskTypes.Select(x => x.Name);
        }

        private IEnumerable<TicketType> _ticketTypes;
        public IEnumerable<TicketType> TicketTypes
        {
            get { return _ticketTypes ?? (_ticketTypes = _cacheDao.GetTicketTypes()); }
        }

        public TicketType GetTicketTypeById(int ticketTypeId)
        {
            if (ticketTypeId == 0) return null;
            return TicketTypes.SingleOrDefault(x => x.Id == ticketTypeId);
        }

        public IEnumerable<TicketType> GetTicketTypes()
        {
            return TicketTypes;
        }

        private IEnumerable<CalculationType> _calculationTypes;
        public IEnumerable<CalculationType> CalculationTypes
        {
            get { return _calculationTypes ?? (_calculationTypes = _cacheDao.GetCalculationTypes()); }
        }

        public CalculationType GetCalculationTypeByName(string name)
        {
            return CalculationTypes.FirstOrDefault(y => y.Name == name);
        }

        private IEnumerable<CalculationSelector> _calculationSelectors;
        public IEnumerable<CalculationSelector> CalculationSelectors
        {
            get { return _calculationSelectors ?? (_calculationSelectors = _cacheDao.GetCalculationSelectors()); }
        }

        public IEnumerable<CalculationSelector> GetCalculationSelectors(int ticketTypeId, int terminalId, int departmentId, int userRoleId)
        {
            var maps = CalculationSelectors.SelectMany(x => x.CalculationSelectorMaps)
                .Where(x => x.TicketTypeId == 0 || x.TicketTypeId == ticketTypeId)
                .Where(x => x.TerminalId == 0 || x.TerminalId == terminalId)
                .Where(x => x.DepartmentId == 0 || x.DepartmentId == departmentId)
                .Where(x => x.UserRoleId == 0 || x.UserRoleId == userRoleId);
            return CalculationSelectors.Where(x => maps.Any(y => y.CalculationSelectorId == x.Id)).OrderBy(x => x.SortOrder);
        }

        private IEnumerable<AutomationCommand> _automationCommands;
        public IEnumerable<AutomationCommand> AutomationCommands
        {
            get { return _automationCommands ?? (_automationCommands = _cacheDao.GetAutomationCommands()); }
        }

        public IEnumerable<AutomationCommandData> GetAutomationCommands(int ticketTypeId, int terminalId, int departmentId, int userRoleId)
        {
            var maps = AutomationCommands.SelectMany(x => x.AutomationCommandMaps)
                .Where(x => x.TicketTypeId == 0 || x.TicketTypeId == ticketTypeId)
                .Where(x => x.TerminalId == 0 || x.TerminalId == terminalId)
                .Where(x => x.DepartmentId == 0 || x.DepartmentId == departmentId)
                .Where(x => x.UserRoleId == 0 || x.UserRoleId == userRoleId);
            var result = maps.Select(x => new AutomationCommandData
                {
                    AutomationCommand = AutomationCommands.First(y => y.Id == x.AutomationCommandId),
                    DisplayOnPayment = x.DisplayOnPayment,
                    DisplayOnTicket = x.DisplayOnTicket,
                    DisplayOnOrders = x.DisplayOnOrders,
                    DisplayOnTicketList = x.DisplayOnTicketList,
                    DisplayUnderTicket = x.DisplayUnderTicket,
                    DisplayUnderTicket2 = x.DisplayUnderTicket2,
                    DisplayOnCommandSelector = x.DisplayOnCommandSelector,
                    EnabledStates = x.EnabledStates,
                    VisibleStates = x.VisibleStates
                });
            return result.OrderBy(x => x.AutomationCommand.SortOrder);
        }

        public AutomationCommand GetAutomationCommandByName(string automationCommand)
        {
            return AutomationCommands.FirstOrDefault(x => x.Name == automationCommand);
        }

        private IEnumerable<AccountType> _accountTypes;
        public IEnumerable<AccountType> AccountTypes
        {
            get { return _accountTypes ?? (_accountTypes = _cacheDao.GetAccountTypes()); }
        }

        public AccountType GetAccountTypeById(int accountTypeId)
        {
            return AccountTypes.Single(x => x.Id == accountTypeId);
        }

        public IEnumerable<AccountType> GetAccountTypes()
        {
            return AccountTypes;
        }

        public IEnumerable<AccountType> GetAccountTypesByName(IEnumerable<string> accountTypeNames)
        {
            return AccountTypes.Where(x => accountTypeNames.Contains(x.Name));
        }

        private IEnumerable<PrintJob> _printJobs;
        public IEnumerable<PrintJob> PrintJobs
        {
            get { return _printJobs ?? (_printJobs = _cacheDao.GetPrintJobs()); }
        }

        public PrintJob GetPrintJobByName(string name)
        {
            return PrintJobs.SingleOrDefault(x => x.Name == name);
        }

        private IEnumerable<EntityType> _entityTypes;
        public IEnumerable<EntityType> EntityTypes
        {
            get { return _entityTypes ?? (_entityTypes = _cacheDao.GetEntityTypes()); }
        }

        public IEnumerable<EntityType> GetEntityTypes()
        {
            return EntityTypes;
        }

        public EntityType GetEntityTypeById(int entityTypeId)
        {
            return EntityTypes.SingleOrDefault(x => x.Id == entityTypeId);
        }

        public EntityType GetEntityTypeByName(string entityTypeName)
        {
            return EntityTypes.SingleOrDefault(x => x.Name == entityTypeName);
        }

        public int GetEntityTypeIdByEntityName(string entityName)
        {
            var rt = EntityTypes.FirstOrDefault(x => x.EntityName == entityName);
            return rt != null ? rt.Id : 0;
        }

        private IEnumerable<Warehouse> _warehouses;
        public IEnumerable<Warehouse> Warehouses
        {
            get { return _warehouses ?? (_warehouses = _cacheDao.GetWarehouses()); }
        }

        public IEnumerable<Warehouse> GetWarehouses()
        {
            return Warehouses;
        }

        private IEnumerable<InventoryTransactionType> _inventoryTransactionTypes;
        public IEnumerable<InventoryTransactionType> InventoryTransactionTypes
        {
            get { return _inventoryTransactionTypes ?? (_inventoryTransactionTypes = _cacheDao.GetInventoryTransactionTypes()); }
        }

        public IEnumerable<InventoryTransactionType> GetInventoryTransactionTypes()
        {
            return InventoryTransactionTypes;
        }

        public EntityScreen GetEntityScreenByName(string screenName)
        {
            return EntityScreens.FirstOrDefault(x => x.Name == screenName);
        }

        public PaymentType GetPaymentTypeByName(string paymentTypeName)
        {
            return PaymentTypes.Single(x => x.Name == paymentTypeName);
        }

        private IEnumerable<Printer> _printers;
        public IEnumerable<Printer> Printers
        {
            get { return _printers ?? (_printers = _printerDao.GetPrinters()); }
        }

        public IEnumerable<Printer> GetPrinters() { return Printers; }

        private IEnumerable<PrinterTemplate> _printerTemplates;
        protected IEnumerable<PrinterTemplate> PrinterTemplates
        {
            get { return _printerTemplates ?? (_printerTemplates = _printerDao.GetPrinterTemplates()); }
        }

        public IEnumerable<PrinterTemplate> GetPrinterTemplates() { return PrinterTemplates; }

        private IEnumerable<State> _states;
        public IEnumerable<State> States
        {
            get { return _states ?? (_states = _cacheDao.GetStates()); }
        }

        public IEnumerable<State> GetStates(int stateType)
        {
            return States.Where(x => x.StateType == stateType);
        }

        public string GetStateColor(string state)
        {
            return States.Any(x => x.Name == state) ? States.Single(x => x.Name == state).Color : "Gainsboro";
        }

        public bool CanShowStateOnTicket(string stateName, string state)
        {
            return States.Any(x => (x.Name == state || x.GroupName == stateName) && x.ShowOnTicket);
        }

        public bool CanShowStateOnProductReport(string stateName, string state)
        {
            return States.Any(x => (x.Name == state || x.GroupName == stateName) && x.ShowOnProductReport);
        }

        public bool CanShowStateOnEndOfDayReport(string stateName, string state)
        {
            return States.Any(x => (x.Name == state || x.GroupName == stateName) && x.ShowOnEndOfDayReport);
        }

        public IEnumerable<EntityType> GetEntityTypesByTicketType(int ticketTypeId)
        {
            return TicketTypes.Single(x => x.Id == ticketTypeId)
                .EntityTypeAssignments
                .OrderBy(x => x.SortOrder)
                .Select(x => GetEntityTypeById(x.EntityTypeId));
        }

        public IEnumerable<Entity> GetEntities(int entityTypeId, string stateData)
        {
            return _entityCache.GetEntities(entityTypeId, stateData);
        }

        public Entity GetEntityByName(string entityTypeName, string entityName)
        {
            var rt = EntityTypes.Single(x => x.Name == entityTypeName);
            var result = _cacheDao.GetEntityByName(rt.Id, entityName);
            return result ?? Entity.GetNullEntity(rt.Id);
        }

        private IEnumerable<Recipe> _recipes;
        public IEnumerable<Recipe> Recipes
        {
            get { return _recipes ?? (_recipes = Dao.Query<Recipe>(x => x.Portion, x => x.RecipeItems, x => x.RecipeItems.Select(y => y.InventoryItem))); }
        }

        public Recipe GetRecipe(string portionName, int menuItemId)
        {
            return Recipes.Single(x => x.Portion.Name == portionName && x.Portion.MenuItemId == menuItemId);
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
            _cacheDao.ResetCache();
            _recipes = null;
            _taxTemplates = null;
            _inventoryTransactionTypes = null;
            _warehouses = null;
            _printers = null;
            _printerTemplates = null;
            _states = null;
            _entityTypes = null;
            _printJobs = null;
            _accountTypes = null;
            _automationCommands = null;
            _calculationSelectors = null;
            _calculationTypes = null;
            _ticketTypes = null;
            _taskTypes = null;
            _screenMenus = null;
            _foreignCurrencies = null;
            _accountScreens = null;
            _entityScreens = null;
            _accountTransactionTypes = null;
            _paymentTypes = null;
            _changePaymentTypes = null;
            _documentTypes = null;
            _ticketTagGroups = null;
            _orderTagGroups = null;
            _productTimers = null;
            _menuItems = null;
            _rules = null;
            _actions = null;
            _entityCache.Reset();
        }
    }

    internal class EntityCache
    {
        private readonly IDictionary<int, IEnumerable<Entity>> _cache = new Dictionary<int, IEnumerable<Entity>>();

        public IEnumerable<Entity> GetEntities(int entityTypeId, string stateData)
        {
            if (!_cache.ContainsKey(entityTypeId))
            {
                var entities = Dao.Query<Entity>(x => x.EntityTypeId == entityTypeId);
                _cache.Add(entityTypeId, entities);
            }

            var stateName = "";
            var state = "";
            if (!string.IsNullOrEmpty(stateData))
            {
                if (stateData.Contains("="))
                {
                    var parts = stateData.Split('=');
                    stateName = parts[0];
                    state = parts[1];
                }
                else
                {
                    stateName = "*";
                    state = stateData;
                }
            }

            if (string.IsNullOrEmpty(stateName)) return _cache[entityTypeId];

            using (var w = WorkspaceFactory.CreateReadOnly())
            {
                var ids = w.Queryable<Entity>().Where(x => x.EntityTypeId == entityTypeId).Select(y => y.Id);
                var entityStates = w.Queryable<EntityStateValue>().Where(x => ids.Contains(x.EntityId)).ToList();
                return
                    _cache[entityTypeId].Where(
                        x =>
                        entityStates.Where(y => y.IsInState(stateName, state)).Select(y => y.EntityId).Contains(x.Id));
            }
        }

        public void Reset()
        {
            _cache.Clear();
        }


    }
}
