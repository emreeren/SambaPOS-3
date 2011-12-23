using System.Collections.Generic;
using Samba.Domain.Models.Locations;
using Samba.Domain.Models.Tickets;
using Samba.Domain.Models.Users;

namespace Samba.Services
{
    public interface IApplicationStateSetter
    {
        void SetCurrentTicket(Ticket ticket);
        void SetCurrentLoggedInUser(User user);
        void SetCurrentDepartment(Department department);
        void SetCurrentDepartment(int departmentId);
        void SetCurrentApplicationScreen(AppScreens appScreen);
        void SetSelectedLocationScreen(LocationScreen locationScreen);
        void ResetWorkPeriods();
    }
}