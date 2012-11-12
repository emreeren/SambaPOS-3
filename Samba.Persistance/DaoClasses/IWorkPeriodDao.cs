using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Domain.Models.Settings;

namespace Samba.Persistance.DaoClasses
{
    public interface IWorkPeriodDao
    {
        void StartWorkPeriod(string description);
        void StopWorkPeriod(string description);
        IEnumerable<WorkPeriod> GetLastWorkPeriods(int count);
    }
}
