using Samba.Domain.Models.Accounts;
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
        LocationList,
        AccountList,
        WorkPeriods,
        Dashboard,
        AccountView,
        ReportScreen
    }

    public interface IApplicationState
    {
        User CurrentLoggedInUser { get; }
        Department CurrentDepartment { get; }
        AppScreens ActiveAppScreen { get; }
        AccountScreen SelectedLocationScreen { get; }
        WorkPeriod CurrentWorkPeriod { get; }
        WorkPeriod PreviousWorkPeriod { get; }
        bool IsCurrentWorkPeriodOpen { get; }
        bool IsLocked { get; }
        Terminal CurrentTerminal { get; }
    }
}
