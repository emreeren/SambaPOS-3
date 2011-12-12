using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Domain.Models.Settings;

namespace Samba.Services
{
    public interface IWorkPeriodService : IService
    {
        WorkPeriod CurrentWorkPeriod { get; }
        WorkPeriod PreviousWorkPeriod { get; }
        IEnumerable<WorkPeriod> LastTwoWorkPeriods { get; }
        bool IsCurrentWorkPeriodOpen { get; }
        void StartWorkPeriod(string description, decimal cashAmount, decimal creditCardAmount, decimal ticketAmount);
        void StopWorkPeriod(string description);
    }
}
