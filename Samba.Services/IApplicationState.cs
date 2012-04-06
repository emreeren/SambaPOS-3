using System.Collections.Generic;
using Samba.Domain.Models.Accounts;
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
        LocationList,
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
        public bool IsFastFood { get { return Model != null ? Model.IsFastFood : false; } }
        public bool IsAlaCarte { get { return Model != null ? Model.IsAlaCarte : false; } }
        public bool IsTakeAway { get { return Model != null ? Model.IsTakeAway : false; } }
        public string PriceTag { get { return Model != null ? Model.PriceTag : ""; } }
        public TicketTemplate TicketTemplate { get { return Model != null ? Model.TicketTemplate : null; } }
        public IList<ResourceScreen> LocationScreens { get { return Model != null ? Model.LocationScreens : null; } }
    }

    public interface IApplicationState
    {
        User CurrentLoggedInUser { get; }
        CurrentDepartmentData CurrentDepartment { get; }
        AppScreens ActiveAppScreen { get; }
        ResourceScreen SelectedResourceScreen { get; }
        WorkPeriod CurrentWorkPeriod { get; }
        WorkPeriod PreviousWorkPeriod { get; }
        bool IsCurrentWorkPeriodOpen { get; }
        bool IsLocked { get; }
        Terminal CurrentTerminal { get; }
    }
}
