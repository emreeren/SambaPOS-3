using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Samba.Domain.Models.Settings;
using Samba.Persistance;
using Samba.Persistance.Data;
using Samba.Services;
using Samba.Services.Common;

namespace Samba.Presentation.Services.Implementations.WorkPeriodModule
{
    [Export(typeof(IWorkPeriodService))]
    public class WorkPeriodService : IWorkPeriodService
    {
        private readonly IApplicationStateSetter _applicationStateSetter;
        private readonly ILogService _logService;
        private readonly IWorkPeriodDao _workPeriodDao;
        private readonly IApplicationState _applicationState;

        [ImportingConstructor]
        public WorkPeriodService(IWorkPeriodDao workPeriodDao, IApplicationState applicationState, IApplicationStateSetter applicationStateSetter, ILogService logService)
        {
            _workPeriodDao = workPeriodDao;
            _applicationState = applicationState;
            _applicationStateSetter = applicationStateSetter;
            _logService = logService;
        }

        [ImportMany]
        public IEnumerable<IWorkPeriodProcessor> WorkPeriodProcessors { get; set; }

        public WorkPeriod CurrentWorkPeriod { get { return _applicationState.CurrentWorkPeriod; } }

        public bool StartWorkPeriod(string description)
        {
            using (var w = WorkspaceFactory.Create())
            {
                using (var tran = w.BeginTransaction())
                {
                    try
                    {
                        _workPeriodDao.StartWorkPeriod(description, w);
                        foreach (var workPeriodProcessor in WorkPeriodProcessors)
                        {
                            workPeriodProcessor.ProcessWorkPeriodStart(CurrentWorkPeriod);
                        }
                        if (tran != null) tran.Commit();
                    }
                    catch (Exception e)
                    {
                        if (tran != null) tran.Rollback();
                        _logService.LogError(e);
                        return false;
                    }
                }
            }
            _applicationStateSetter.ResetWorkPeriods();
            return true;
        }

        public bool StopWorkPeriod(string description)
        {
            using (var w = WorkspaceFactory.Create())
            {
                using (var tran = w.BeginTransaction())
                {
                    try
                    {
                        _workPeriodDao.StopWorkPeriod(description, w);
                        foreach (var workPeriodProcessor in WorkPeriodProcessors)
                        {
                            workPeriodProcessor.ProcessWorkPeriodEnd(CurrentWorkPeriod);
                        }
                        if (tran != null) tran.Commit();
                    }
                    catch (Exception e)
                    {
                        if (tran != null) tran.Rollback();
                        _logService.LogError(e);
                        return false;
                    }
                }
            }
            _applicationStateSetter.ResetWorkPeriods();
            return true;
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
