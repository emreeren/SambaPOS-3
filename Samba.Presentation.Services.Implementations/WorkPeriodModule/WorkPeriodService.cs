using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Samba.Domain.Models.Settings;
using Samba.Persistance.DaoClasses;

namespace Samba.Presentation.Services.Implementations.WorkPeriodModule
{
    [Export(typeof(IWorkPeriodService))]
    public class WorkPeriodService : IWorkPeriodService
    {
        private readonly IApplicationStateSetter _applicationStateSetter;
        private readonly IWorkPeriodDao _workPeriodDao;
        private readonly IApplicationState _applicationState;

        [ImportingConstructor]
        public WorkPeriodService(IWorkPeriodDao workPeriodDao, IApplicationState applicationState, IApplicationStateSetter applicationStateSetter)
        {
            _workPeriodDao = workPeriodDao;
            _applicationState = applicationState;
            _applicationStateSetter = applicationStateSetter;
        }

        public WorkPeriod CurrentWorkPeriod { get { return _applicationState.CurrentWorkPeriod; } }

        public void StartWorkPeriod(string description)
        {
            _applicationStateSetter.ResetWorkPeriods();
            _workPeriodDao.StartWorkPeriod(description);
            _applicationStateSetter.ResetWorkPeriods();
        }

        public void StopWorkPeriod(string description)
        {
            _workPeriodDao.StopWorkPeriod(description);
            _applicationStateSetter.ResetWorkPeriods();
        }

        public IEnumerable<WorkPeriod> GetLastWorkPeriods(int count)
        {
            return _workPeriodDao.GetLastWorkPeriods(count);
        }

        public DateTime GetWorkPeriodStartDate()
        {
            return CurrentWorkPeriod.StartDate;
        }
    }
}
