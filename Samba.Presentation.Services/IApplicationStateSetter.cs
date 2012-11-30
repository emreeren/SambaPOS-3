﻿using System.Collections.Generic;
using Samba.Domain.Models.Resources;
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
        ResourceScreen SetSelectedResourceScreen(ResourceScreen resourceScreen);
        void SetApplicationLocked(bool isLocked);
        void SetNumberpadValue(string value);
        void SetLastPaidItems(IEnumerable<PaidItem> paidItems);
        void SetCurrentTicketType(TicketType ticketType);
        void ResetWorkPeriods();
    }
}