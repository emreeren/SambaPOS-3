using System.Collections.Generic;
using Samba.Domain.Models.Settings;

namespace Samba.Persistance
{
    public interface IWorkPeriodDao
    {
        void StartWorkPeriod(string description);
        void StopWorkPeriod(string description);
        IEnumerable<WorkPeriod> GetLastWorkPeriods(int count);
    }
}
