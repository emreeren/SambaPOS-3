using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Linq.Expressions;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Actions;
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
            get { return _menuItems ?? (_menuItems = Dao.Query<MenuItem>(x => x.TaxTemplate.AccountTransactionTemplate, x => x.Portions.Select(y => y.Prices))); }
        }

        public MenuItem GetMenuItem(Expression<Func<MenuItem, bool>> expression)
        {
            return MenuItems.Single(expression.Compile());
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

        public IEnumerable<string> GetTicketTagGroupNames()
        {
            return TicketTagGroups.Select(x => x.Name).Distinct();
        }

        public TicketTagGroup GetTicketTagGroupById(int id)
        {
            return TicketTagGroups.FirstOrDefault(x => x.Id == id);
        }

        private IEnumerable<AccountTransactionTemplate> _accountTransactionTemplates;
        public IEnumerable<AccountTransactionTemplate> AccountTransactionTemplates
        {
            get { return _accountTransactionTemplates ?? (_accountTransactionTemplates = Dao.Query<AccountTransactionTemplate>()); }
        }

        public AccountTransactionTemplate GetAccountTransactionTemplateById(int id)
        {
            return AccountTransactionTemplates.Single(x => x.Id == id);
        }

        private IEnumerable<Resource> _resources;
        public IEnumerable<Resource> Resources
        {
            get { return _resources ?? (_resources = Dao.Query<Resource>()); }
        }

        public IEnumerable<Resource> GetResourcesByTemplateId(int templateId)
        {
            return Resources.Where(x => x.ResourceTemplateId == templateId);
        }

        private IEnumerable<ResourceTemplate> _resourceTemplates;
        public IEnumerable<ResourceTemplate> ResourceTemplates
        {
            get { return _resourceTemplates ?? (_resourceTemplates = Dao.Query<ResourceTemplate>(x => x.ResoruceCustomFields).OrderBy(x => x.Order)); }
        }

        private IEnumerable<AccountTemplate> _accountTemplates;
        public IEnumerable<AccountTemplate> AccountTemplates
        {
            get { return _accountTemplates ?? (_accountTemplates = Dao.Query<AccountTemplate>()); }
        }

        public IEnumerable<ResourceTemplate> GetResourceTemplates()
        {
            return ResourceTemplates;
        }

        public ResourceTemplate GetResourceTemplateById(int resourceTemplateId)
        {
            return ResourceTemplates.Single(x => x.Id == resourceTemplateId);
        }

        public AccountTemplate GetAccountTemplateById(int accountTemplateId)
        {
            return AccountTemplates.Single(x => x.Id == accountTemplateId);
        }

        public Account GetAccountById(int accountId)
        {
            return Dao.SingleWithCache<Account>(x => x.Id == accountId);
        }

        public Resource GetResourceById(int accountId)
        {
            return Dao.SingleWithCache<Resource>(x => x.Id == accountId);
        }

        private IEnumerable<AccountTransactionDocumentTemplate> _documentTemplates;
        public IEnumerable<AccountTransactionDocumentTemplate> DocumentTemplates { get { return _documentTemplates ?? (_documentTemplates = Dao.Query<AccountTransactionDocumentTemplate>(x => x.TransactionTemplates)); } }

        public IEnumerable<AccountTransactionDocumentTemplate> GetAccountTransactionDocumentTemplates(int accountTemplateId)
        {
            return DocumentTemplates.Where(x => x.MasterAccountTemplateId == accountTemplateId);
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

        private IEnumerable<PaymentTemplate> _paymentTemplates;
        public IEnumerable<PaymentTemplate> PaymentTemplates
        {
            get { return _paymentTemplates ?? (_paymentTemplates = Dao.Query<PaymentTemplate>(x => x.PaymentTemplateMaps, x => x.AccountTransactionTemplate, x => x.Account)); }
        }

        public IEnumerable<PaymentTemplate> GetUnderTicketPaymentTemplates()
        {
            var maps = PaymentTemplates.SelectMany(x => x.PaymentTemplateMaps)
                .Where(x => x.DisplayUnderTicket)
                .Where(x => x.TerminalId == 0 || x.TerminalId == _applicationState.CurrentTerminal.Id)
                .Where(x => x.DepartmentId == 0 || x.DepartmentId == _applicationState.CurrentDepartment.Id)
                .Where(x => x.UserRoleId == 0 || x.UserRoleId == _applicationState.CurrentLoggedInUser.UserRole.Id);
            return PaymentTemplates.Where(x => maps.Any(y => y.PaymentTemplateId == x.Id)).OrderBy(x => x.Order);
        }

        public IEnumerable<PaymentTemplate> GetPaymentScreenPaymentTemplates()
        {
            var maps = PaymentTemplates.SelectMany(x => x.PaymentTemplateMaps)
                .Where(x => x.DisplayAtPaymentScreen)
                .Where(x => x.TerminalId == 0 || x.TerminalId == _applicationState.CurrentTerminal.Id)
                .Where(x => x.DepartmentId == 0 || x.DepartmentId == _applicationState.CurrentDepartment.Id)
                .Where(x => x.UserRoleId == 0 || x.UserRoleId == _applicationState.CurrentLoggedInUser.UserRole.Id);
            return PaymentTemplates.Where(x => maps.Any(y => y.PaymentTemplateId == x.Id)).OrderBy(x => x.Order);
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
            var maps = AutomationCommands.SelectMany(x => x.AutomationCommandMaps)
                .Where(x => x.TerminalId == 0 || x.TerminalId == _applicationState.CurrentTerminal.Id)
                .Where(x => x.DepartmentId == 0 || x.DepartmentId == _applicationState.CurrentDepartment.Id)
                .Where(x => x.UserRoleId == 0 || x.UserRoleId == _applicationState.CurrentLoggedInUser.UserRole.Id);
            var result = maps.Select(x => new AutomationCommandData { AutomationCommand = AutomationCommands.First(y => y.Id == x.AutomationCommandId), DisplayOnPayment = x.DisplayOnPayment, DisplayOnTicket = x.DisplayOnTicket, VisualBehaviour = x.VisualBehaviour });
            return result.OrderBy(x => x.AutomationCommand.Order);
        }

        private IEnumerable<CalculationTemplate> _calculationTemplates;
        public IEnumerable<CalculationTemplate> CalculationTemplates
        {
            get { return _calculationTemplates ?? (_calculationTemplates = Dao.Query<CalculationTemplate>(x => x.AccountTransactionTemplate, x => x.CalculationTemplateMaps)); }
        }

        public IEnumerable<CalculationTemplate> GetCalculationTemplates()
        {
            var maps = CalculationTemplates.SelectMany(x => x.CalculationTemplateMaps)
                .Where(x => x.TerminalId == 0 || x.TerminalId == _applicationState.CurrentTerminal.Id)
                .Where(x => x.DepartmentId == 0 || x.DepartmentId == _applicationState.CurrentDepartment.Id)
                .Where(x => x.UserRoleId == 0 || x.UserRoleId == _applicationState.CurrentLoggedInUser.UserRole.Id);
            return CalculationTemplates.Where(x => maps.Any(y => y.CalculationTemplateId == x.Id)).OrderBy(x => x.Order);
        }

        public IEnumerable<AccountTemplate> GetAccountTemplates()
        {
            return AccountTemplates;
        }

        private IEnumerable<AccountScreen> _accountScreens;
        public IEnumerable<AccountScreen> AccountScreens
        {
            get { return _accountScreens ?? (_accountScreens = Dao.Query<AccountScreen>()); }
        }

        public IEnumerable<AccountScreen> GetAccountScreens()
        {
            return AccountScreens;
        }

        public IEnumerable<AccountTemplate> GetAccountTemplatesByName(IEnumerable<string> accountTemplateNames)
        {
            return AccountTemplates.Where(x => accountTemplateNames.Contains(x.Name));
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

        public override void Reset()
        {
            _menuItems = null;
            _screenMenus = null;
            _accountTransactionTemplates = null;
            _accountScreens = null;
            _calculationTemplates = null;
            _automationCommands = null;
            _orderTagGroups = null;
            _resourceTemplates = null;
            _accountTemplates = null;
            _resources = null;
            _documentTemplates = null;
            _resourceStates = null;
            _printJobs = null;
            _paymentTemplates = null;
            _ticketTagGroups = null;
            Dao.ResetCache();
        }
    }
}
