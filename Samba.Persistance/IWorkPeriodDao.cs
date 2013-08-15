using System.Collections.Generic;
using Samba.Domain.Models.Settings;
using Samba.Infrastructure.Data;

namespace Samba.Persistance
{
    public interface IWorkPeriodDao
    {
        void StartWorkPeriod(string description,IWorkspace workspace);
        void StopWorkPeriod(string description, IWorkspace workspace);
        IEnumerable<WorkPeriod> GetLastWorkPeriods(int count);
    }
}
