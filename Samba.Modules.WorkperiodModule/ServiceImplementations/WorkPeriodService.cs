using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Samba.Domain.Models.Settings;
using Samba.Persistance.Data;
using Samba.Presentation.Common.Services;
using Samba.Services;

namespace Samba.Modules.WorkperiodModule.ServiceImplementations
{
    [Export(typeof(IWorkPeriodService))]
    public class WorkPeriodService : AbstractService, IWorkPeriodService
    {
        private IEnumerable<WorkPeriod> _lastTwoWorkPeriods;
        public IEnumerable<WorkPeriod> LastTwoWorkPeriods
        {
            get { return _lastTwoWorkPeriods ?? (_lastTwoWorkPeriods = Dao.Last<WorkPeriod>(2)); }
        }

        public WorkPeriod CurrentWorkPeriod
        {
            get { return LastTwoWorkPeriods.LastOrDefault(); }
        }

        public WorkPeriod PreviousWorkPeriod
        {
            get { return LastTwoWorkPeriods.Count() > 1 ? LastTwoWorkPeriods.FirstOrDefault() : null; }
        }

        public bool IsCurrentWorkPeriodOpen
        {
            get
            {
                return CurrentWorkPeriod != null &&
                    CurrentWorkPeriod.StartDate == CurrentWorkPeriod.EndDate;
            }
        }

        public void StartWorkPeriod(string description, decimal cashAmount, decimal creditCardAmount, decimal ticketAmount)
        {
            using (var workspace = WorkspaceFactory.Create())
            {
                _lastTwoWorkPeriods = null;

                var latestWorkPeriod = workspace.Last<WorkPeriod>();
                if (latestWorkPeriod != null && latestWorkPeriod.StartDate == latestWorkPeriod.EndDate)
                {
                    return;
                }

                var now = DateTime.Now;
                var newPeriod = new WorkPeriod
                {
                    StartDate = now,
                    EndDate = now,
                    StartDescription = description,
                    CashAmount = cashAmount,
                    CreditCardAmount = creditCardAmount,
                    TicketAmount = ticketAmount
                };

                workspace.Add(newPeriod);
                workspace.CommitChanges();
                _lastTwoWorkPeriods = null;
            }
        }

        public void StopWorkPeriod(string description)
        {
            using (var workspace = WorkspaceFactory.Create())
            {
                var period = workspace.Last<WorkPeriod>();
                if (period.EndDate == period.StartDate)
                {
                    period.EndDate = DateTime.Now;
                    period.EndDescription = description;
                    workspace.CommitChanges();
                }
                _lastTwoWorkPeriods = null;
            }
        }

        public override void Reset()
        {
            _lastTwoWorkPeriods = null;
        }
    }
}
