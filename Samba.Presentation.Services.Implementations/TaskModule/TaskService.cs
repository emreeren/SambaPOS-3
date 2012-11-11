using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Omu.ValueInjecter;
using Samba.Domain.Models.Tasks;
using Samba.Infrastructure.Data;
using Samba.Persistance.Data;

namespace Samba.Presentation.Services.Implementations.TaskModule
{
    [Export(typeof(ITaskService))]
    public class TaskService : ITaskService
    {
        private readonly IWorkPeriodService _workPeriodService;
        private readonly TaskParser _taskParser;
        private DateTime _lastReadTime;
        private readonly TaskCache _taskCache;

        [ImportingConstructor]
        public TaskService(IWorkPeriodService workPeriodService, TaskParser taskParser)
        {
            _workPeriodService = workPeriodService;
            _taskParser = taskParser;
            _lastReadTime = DateTime.MinValue;
            _taskCache = new TaskCache();
        }

        public Task AddNewTask(int taskTypeId, string taskContent)
        {
            if (taskTypeId == 0) return null;
            using (var w = WorkspaceFactory.Create())
            {
                var task = new Task { Content = taskContent, TaskTypeId = taskTypeId };
                var tokens = _taskParser.Parse(task);
                foreach (var taskToken in tokens)
                    task.TaskTokens.Add(taskToken);
                w.Add(task);
                w.CommitChanges();
                return task;
            }
        }

        public IEnumerable<Task> GetTasks(int taskTypeId, int timeDelta = 0)
        {
            if (timeDelta == 0 || Dao.Exists<Task>(x => x.TaskTypeId == taskTypeId && x.LastUpdateTime > _lastReadTime))
            {
                var wpDate = _workPeriodService.GetWorkPeriodStartDate();
                var result = Dao.Query<Task>(x => x.TaskTypeId == taskTypeId && (x.EndDate > wpDate || !x.Completed), x => x.TaskTokens);
                _taskCache.InjectFrom<EntityInjection>(new { Tasks = result });
                _lastReadTime = DateTime.Now.AddSeconds(-timeDelta);
            }
            return _taskCache.Tasks;
        }

        public void SaveTask(Task task)
        {
            Dao.Save(task);
        }

        public IEnumerable<Task> SaveTasks(int taskTypeId, IEnumerable<Task> tasks, int timeDelta = 0)
        {
            tasks.Where(x => x.LastUpdateTime > _lastReadTime).ToList().ForEach(SaveTask);
            return GetTasks(taskTypeId, timeDelta);
        }
    }
}