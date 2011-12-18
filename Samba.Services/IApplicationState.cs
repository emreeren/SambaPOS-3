using System.Collections.Generic;
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
        CashView,
        ReportScreen
    }

    public interface IApplicationState
    {
        Ticket CurrentTicket { get; }
        User CurrentLoggedInUser { get; }
        IEnumerable<Department> Departments { get; }
        Department CurrentDepartment { get; }
        AppScreens ActiveAppScreen { get; }
    }
}
