using System.Collections.Generic;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Menus;
using Samba.Domain.Models.Resources;
using Samba.Domain.Models.Settings;
using Samba.Domain.Models.Tickets;
using Samba.Domain.Models.Users;
using Samba.Services.Common;

namespace Samba.Presentation.Services
{
    public enum AppScreens
    {
        LoginScreen,
        Navigation,
        SingleTicket,
        TicketList,
        Payment,
        ResourceView,
        AccountList,
        WorkPeriods,
        Dashboard,
        AccountView,
        ReportScreen
    }

    public class CurrentDepartmentData
    {
        public Department Model { get; set; }
        public int Id { get { return Model != null ? Model.Id : 0; } }
        public string Name { get { return Model != null ? Model.Name : ""; } }
        public string PriceTag { get { return Model != null ? Model.PriceTag : ""; } }
        public int TicketCreationMethod { get { return Model != null ? Model.TicketCreationMethod : 0; } }
        public int TicketTypeId { get { return Model != null ? Model.TicketTypeId : 0; } }
    }

    public interface IApplicationState
    {
        string NumberPadValue { get; }
        User CurrentLoggedInUser { get; }
        CurrentDepartmentData CurrentDepartment { get; }
        TicketType CurrentTicketType { get; set; }
        AppScreens ActiveAppScreen { get; }
        ResourceScreen SelectedResourceScreen { get; }
        ResourceScreen ActiveResourceScreen { get; }
        WorkPeriod CurrentWorkPeriod { get; }
        WorkPeriod PreviousWorkPeriod { get; }
        bool IsCurrentWorkPeriodOpen { get; }
        bool IsLocked { get; }
        Terminal CurrentTerminal { get; }
        IEnumerable<PaidItem> LastPaidItems { get; }

        ProductTimer GetProductTimer(int menuItemId);
        IEnumerable<OrderTagGroup> GetOrderTagGroups(params int[] menuItemIds);
        IEnumerable<OrderStateGroup> GetOrderStateGroups(params int[] menuItemIds);
        IEnumerable<AccountTransactionDocumentType> GetAccountTransactionDocumentTypes(int accountTypeId);
        IEnumerable<AccountTransactionDocumentType> GetBatchDocumentTypes(IEnumerable<string> accountTypeNamesList);
        IEnumerable<PaymentType> GetUnderTicketPaymentTypes();
        IEnumerable<PaymentType> GetPaymentScreenPaymentTypes();
        IEnumerable<ChangePaymentType> GetChangePaymentTypes();
        IEnumerable<TicketTagGroup> GetTicketTagGroups();
        IEnumerable<AutomationCommandData> GetAutomationCommands();
        IEnumerable<CalculationSelector> GetCalculationSelectors();
        IEnumerable<ResourceScreen> GetResourceScreens();
        IEnumerable<ResourceScreen> GetTicketResourceScreens();

        void ResetState();
    }
}
