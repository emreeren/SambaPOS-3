using System;
using System.Collections.Generic;
using Samba.Domain.Models.Settings;
using Samba.Presentation.Services.Common;

namespace Samba.Presentation.Services
{
    public interface IWorkPeriodService : IPresentationService
    {
        void StartWorkPeriod(string description);
        void StopWorkPeriod(string description);
        IEnumerable<WorkPeriod> GetLastWorkPeriods(int count);
        DateTime GetWorkPeriodStartDate();
    }
}
