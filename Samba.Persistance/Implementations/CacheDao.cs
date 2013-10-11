using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Automation;
using Samba.Domain.Models.Entities;
using Samba.Domain.Models.Inventory;
using Samba.Domain.Models.Menus;
using Samba.Domain.Models.Settings;
using Samba.Domain.Models.Tasks;
using Samba.Domain.Models.Tickets;
using Samba.Domain.Models.Users;
using Samba.Infrastructure.Data;
using Samba.Infrastructure.Data.Validation;
using Samba.Localization.Properties;
using Samba.Persistance.Data;

namespace Samba.Persistance.Implementations
{
    [Export(typeof(ICacheDao))]
    class CacheDao : ICacheDao
    {
        [ImportingConstructor]
        public CacheDao()
        {
            ValidatorRegistry.RegisterDeleteValidator<Department>(
                x => Dao.Exists<UserRole>(y => y.DepartmentId == x.Id), Resources.Department, Resources.UserRole);
        }

        public void ResetCache()
        {
            Dao.ResetCache();
        }

        public IEnumerable<AppRule> GetRules()
        {
            return Dao.Query<AppRule>(x => x.Actions, x => x.AppRuleMaps).OrderBy(x => x.SortOrder);
        }

        public IEnumerable<AppAction> GetActions()
        {
            return Dao.Query<AppAction>().OrderBy(x => x.SortOrder);
        }

        public IEnumerable<MenuItem> GetMenuItems()
        {
            return Dao.Query<MenuItem>(x => x.Portions.Select(y => y.Prices));
        }

        public IEnumerable<ProductTimer> GetProductTimers()
        {
            return Dao.Query<ProductTimer>(x => x.ProductTimerMaps);
        }

        public IEnumerable<OrderTagGroup> GetOrderTagGroups()
        {
            return Dao.Query<OrderTagGroup>(x => x.OrderTags, x => x.OrderTagMaps);
        }

        public IEnumerable<AccountTransactionType> GetAccountTransactionTypes()
        {
            return Dao.Query<AccountTransactionType>();
        }

        public IEnumerable<Entity> GetEntities(int entityTypeId)
        {
            return Dao.Query<Entity>(x => x.EntityTypeId == entityTypeId);
        }

        public IEnumerable<EntityType> GetEntityTypes()
        {
            return Dao.Query<EntityType>(x => x.EntityCustomFields).OrderBy(x => x.SortOrder);
        }

        public IEnumerable<AccountType> GetAccountTypes()
        {
            return Dao.Query<AccountType>().OrderBy(x => x.SortOrder);
        }

        public IEnumerable<AccountTransactionDocumentType> GetAccountTransactionDocumentTypes()
        {
            return Dao.Query<AccountTransactionDocumentType>(x => x.TransactionTypes, x => x.AccountTransactionDocumentTypeMaps, x => x.AccountTransactionDocumentAccountMaps);
        }

        public IEnumerable<State> GetStates()
        {
            return Dao.Query<State>();
        }

        public IEnumerable<PrintJob> GetPrintJobs()
        {
            return Dao.Query<PrintJob>(x => x.PrinterMaps);
        }

        public IEnumerable<PaymentType> GetPaymentTypes()
        {
            return Dao.Query<PaymentType>(x => x.PaymentTypeMaps, x => x.AccountTransactionType, x => x.Account);
        }

        public IEnumerable<ChangePaymentType> GetChangePaymentTypes()
        {
            return Dao.Query<ChangePaymentType>(x => x.ChangePaymentTypeMaps, x => x.AccountTransactionType, x => x.Account);
        }

        public IEnumerable<TicketTagGroup> GetTicketTagGroups()
        {
            return Dao.Query<TicketTagGroup>(x => x.TicketTags, x => x.TicketTagMaps);
        }

        public IEnumerable<AutomationCommand> GetAutomationCommands()
        {
            return Dao.Query<AutomationCommand>(x => x.AutomationCommandMaps);
        }

        public IEnumerable<CalculationSelector> GetCalculationSelectors()
        {
            return Dao.Query<CalculationSelector>(x => x.CalculationSelectorMaps, x => x.CalculationTypes.Select(y => y.AccountTransactionType));
        }

        public IEnumerable<CalculationType> GetCalculationTypes()
        {
            return Dao.Query<CalculationType>(x => x.AccountTransactionType);
        }

        public IEnumerable<AccountScreen> GetAccountScreens()
        {
            return Dao.Query<AccountScreen>(x => x.AccountScreenValues).OrderBy(x => x.SortOrder);
        }

        public IEnumerable<ScreenMenu> GetScreenMenus()
        {
            var time = DateTime.Now.Ticks;
            var result = Dao.Query<ScreenMenu>(x => x.Categories.Select(z => z.ScreenMenuItems));
            var time2 = DateTime.Now.Ticks;
            Debug.WriteLine("Screen Menu: " + new TimeSpan(time2 - time).TotalMilliseconds);
            return result;
        }

        public IEnumerable<EntityScreen> GetEntityScreens()
        {
            return Dao.Query<EntityScreen>(x => x.EntityScreenMaps, x => x.ScreenItems, x => x.Widgets);
        }

        public IEnumerable<TicketType> GetTicketTypes()
        {
            return Dao.Query<TicketType>(x => x.SaleTransactionType, x => x.OrderNumerator, x => x.TicketNumerator, x => x.EntityTypeAssignments, x => x.MenuAssignments).OrderBy(x => x.SortOrder);
        }

        public IEnumerable<TaskType> GetTaskTypes()
        {
            return Dao.Query<TaskType>(x => x.TaskCustomFields);
        }

        public IEnumerable<ForeignCurrency> GetForeignCurrencies()
        {
            return Dao.Query<ForeignCurrency>();
        }

        public IEnumerable<Department> GetDepartments()
        {
            return Dao.Query<Department>().OrderBy(x => x.SortOrder).ThenBy(x => x.Id);
        }

        public Entity GetEntityByName(int entityTypeId, string entitiyName)
        {
            return Dao.SingleWithCache<Entity>(x => x.Name == entitiyName && x.EntityTypeId == entityTypeId);
        }

        public IEnumerable<TaxTemplate> GetTaxTemplates()
        {
            return Dao.Query<TaxTemplate>(x => x.AccountTransactionType, x => x.TaxTemplateMaps);
        }

        public IEnumerable<Warehouse> GetWarehouses()
        {
            return Dao.Query<Warehouse>();
        }

        public IEnumerable<InventoryTransactionType> GetInventoryTransactionTypes()
        {
            return Dao.Query<InventoryTransactionType>();
        }
    }
}
