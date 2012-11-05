using System.Collections.Generic;
using Samba.Domain.Models.Resources;
using Samba.Domain.Models.Settings;
using Samba.Domain.Models.Tickets;
using Samba.Domain.Models.Users;

namespace Samba.Services
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
    }

    public interface IApplicationState
    {
        string NumberPadValue { get; }
        User CurrentLoggedInUser { get; }
        CurrentDepartmentData CurrentDepartment { get; }
        TicketType CurrentTicketType { get; set; }
        AppScreens ActiveAppScreen { get; }
        ResourceScreen SelectedResourceScreen { get; }
        WorkPeriod CurrentWorkPeriod { get; }
        WorkPeriod PreviousWorkPeriod { get; }
        bool IsCurrentWorkPeriodOpen { get; }
        bool IsLocked { get; }
        Terminal CurrentTerminal { get; }
        IEnumerable<PaidItem> LastPaidItems { get; }
        void ResetState();
    }
}
