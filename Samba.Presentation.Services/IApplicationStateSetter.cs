using System.Collections.Generic;
using Samba.Domain.Models.Entities;
using Samba.Domain.Models.Tickets;
using Samba.Domain.Models.Users;
using Samba.Presentation.Services.Common;

namespace Samba.Presentation.Services
{
    public interface IApplicationStateSetter
    {
        void SetCurrentLoggedInUser(User user);
        void SetCurrentDepartment(int departmentId);
        void SetCurrentApplicationScreen(AppScreens appScreen);
        EntityScreen SetSelectedEntityScreen(EntityScreen entityScreen);
        void SetApplicationLocked(bool isLocked);
        void SetNumberpadValue(string value);
        void SetCurrentTicketType(TicketType ticketType);
        void SetCurrentTerminal(string terminalName);
        void ResetWorkPeriods();
    }
}