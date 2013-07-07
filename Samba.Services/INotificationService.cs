using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Services.Common;

namespace Samba.Services
{
    public interface INotificationService
    {
        void NotifyEvent(string eventName, object dataObject, int terminalId, int departmentId, int userRoleId, int ticketTypeId, Action<ActionData> dataAction);
    }
}
