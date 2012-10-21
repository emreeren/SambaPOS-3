using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Samba.Domain.Models.Settings;
using Samba.Persistance.Data;
using Samba.Services.Common;

namespace Samba.Services.Implementations.WorkPeriodModule
{
    [Export(typeof(IWorkPeriodService))]
    public class WorkPeriodService : AbstractService, IWorkPeriodService
    {
        private readonly IApplicationStateSetter _applicationStateSetter;
        private readonly IApplicationState _applicationState;

        [ImportingConstructor]
        public WorkPeriodService(IApplicationState applicationState, IApplicationStateSetter applicationStateSetter)
        {
            _applicationState = applicationState;
            _applicationStateSetter = applicationStateSetter;
        }

        public WorkPeriod CurrentWorkPeriod { get { return _applicationState.CurrentWorkPeriod; } }

        public void StartWorkPeriod(string description)
        {
            using (var workspace = WorkspaceFactory.Create())
            {
                _applicationStateSetter.ResetWorkPeriods();

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
                };

                workspace.Add(newPeriod);
                workspace.CommitChanges();
                _applicationStateSetter.ResetWorkPeriods();
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
                _applicationStateSetter.ResetWorkPeriods();
            }
        }

        public IEnumerable<WorkPeriod> GetLastWorkPeriods(int count)
        {
            return Dao.Last<WorkPeriod>(count);
        }

        public override void Reset()
        {

        }
    }
}
