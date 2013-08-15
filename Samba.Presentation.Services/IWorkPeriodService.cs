using System;
using System.Collections.Generic;
using Samba.Domain.Models.Settings;

namespace Samba.Presentation.Services
{
    public interface IWorkPeriodService 
    {
        bool StartWorkPeriod(string description);
        bool StopWorkPeriod(string description);
        IEnumerable<WorkPeriod> GetLastWorkPeriods(int count);
        DateTime GetWorkPeriodStartDate();
    }
}
