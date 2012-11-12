using System;
using System.Collections.Generic;
using Samba.Domain.Models.Settings;

namespace Samba.Presentation.Services
{
    public interface IWorkPeriodService 
    {
        void StartWorkPeriod(string description);
        void StopWorkPeriod(string description);
        IEnumerable<WorkPeriod> GetLastWorkPeriods(int count);
        DateTime GetWorkPeriodStartDate();
    }
}
