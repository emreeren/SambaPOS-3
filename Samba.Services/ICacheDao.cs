using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Automation;
using Samba.Domain.Models.Menus;
using Samba.Domain.Models.Resources;
using Samba.Domain.Models.Settings;
using Samba.Domain.Models.Tasks;
using Samba.Domain.Models.Tickets;

namespace Samba.Services
{
    public interface ICacheDao
    {
        void ResetCache();
        IEnumerable<MenuItem> GetMenuItems();
        IEnumerable<ProductTimer> GetProductTimers();
        IEnumerable<OrderTagGroup> GetOrderTagGroups();
        IEnumerable<OrderStateGroup> GetOrderStateGroups();
        IEnumerable<AccountTransactionType> GetAccountTransactionTypes();
        IEnumerable<Resource> GetResources();
        IEnumerable<ResourceType> GetResourceTypes();
        IEnumerable<AccountType> GetAccountTypes();
        IEnumerable<AccountTransactionDocumentType> GetAccountTransactionDocumentTypes();
        IEnumerable<ResourceState> GetResourceStates();
        IEnumerable<PrintJob> GetPrintJobs();
        IEnumerable<PaymentType> GetPaymentTypes();
        IEnumerable<ChangePaymentType> GetChangePaymentTypes();
        IEnumerable<TicketTagGroup> GetTicketTagGroups();
        IEnumerable<AutomationCommand> GetAutomationCommands();
        IEnumerable<CalculationSelector> GetCalculationSelectors();
        IEnumerable<AccountScreen> GetAccountScreens();
        IEnumerable<ScreenMenu> GetScreenMenus();
        IEnumerable<ResourceScreen> GetResourceScreens();
        IEnumerable<TicketType> GetTicketTypes();
        IEnumerable<TaskType> GetTaskTypes();
        IEnumerable<ForeignCurrency> GetForeignCurrencies();
        IEnumerable<Department> GetDepartments();
    }
}
