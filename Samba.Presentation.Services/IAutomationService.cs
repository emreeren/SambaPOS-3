using System;
using System.Collections.Generic;
using Samba.Domain.Models.Automation;
using Samba.Presentation.Services.Common;

namespace Samba.Presentation.Services
{
    public interface IAutomationService
    {
        void NotifyEvent(string eventName, object dataObject); 
    }
}
