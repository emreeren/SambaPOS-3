using System.Collections.Generic;
using Samba.Domain.Models.Settings;
using Samba.Services.Common;

namespace Samba.Services
{
    public interface IWorkPeriodService : IService
    {
        void StartWorkPeriod(string description, decimal cashAmount, decimal creditCardAmount, decimal ticketAmount);
        void StopWorkPeriod(string description);
        IEnumerable<WorkPeriod> GetLastWorkPeriods(int count);
    }
}
