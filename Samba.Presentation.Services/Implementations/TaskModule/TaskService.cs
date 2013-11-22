using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Runtime.CompilerServices;
using Omu.ValueInjecter;
using Samba.Domain.Models.Tasks;
using Samba.Infrastructure.Data.Injection;
using Samba.Persistance;
using Samba.Persistance.Data;

namespace Samba.Presentation.Services.Implementations.TaskModule
{
    [Export(typeof(ITaskService))]
    public class TaskService : ITaskService
    {
        private readonly ITaskDao _taskDao;
        private readonly IWorkPeriodService _workPeriodService;
        private readonly TaskParser _taskParser;
        private DateTime _lastReadTime;
        private readonly TaskCache _taskCache;

        [ImportingConstructor]
        public TaskService(ITaskDao taskDao, IWorkPeriodService workPeriodService, TaskParser taskParser)
        {
            _taskDao = taskDao;
            _workPeriodService = workPeriodService;
            _taskParser = taskParser;
            _lastReadTime = DateTime.MinValue;
            _taskCache = new TaskCache();
        }

        public Task AddNewTask(int taskTypeId, string taskContent, Dictionary<string, string> customFields, bool saveTask)
        {
            if (taskTypeId == 0) return null;
            var task = new Task { Content = taskContent, TaskTypeId = taskTypeId };
            var tokens = _taskParser.Parse(task);
            if (tokens != null)
            {
                foreach (var taskToken in tokens)
                    task.TaskTokens.Add(taskToken);
            }
            foreach (var customField in customFields)
            {
                task.UpdateCustomDataValue(customField.Key, customField.Value);
            }
            if (saveTask) SaveTask(task);
            return task;
        }

        public IEnumerable<Task> GetTasks(int taskTypeId, int timeDelta = 0)
        {
            if (timeDelta == 0 || Dao.Exists<Task>(x => x.TaskTypeId == taskTypeId && x.LastUpdateTime > _lastReadTime))
            {
                var wpDate = _workPeriodService.GetWorkPeriodStartDate();
                var result = _taskDao.GetTasks(taskTypeId, wpDate);
                _taskCache.InjectFrom<EntityInjection>(new { Tasks = result });
                _lastReadTime = DateTime.Now.AddSeconds(-timeDelta);
            }
            return _taskCache.Tasks;
        }

        public void SaveTask(Task task)
        {
            _taskDao.SaveTask(task);
        }

        [MethodImplAttribute(MethodImplOptions.Synchronized)]
        public IEnumerable<Task> SaveTasks(int taskTypeId, IEnumerable<Task> tasks, int timeDelta = 0)
        {
            tasks.Where(x => x.LastUpdateTime > _lastReadTime).ToList().ForEach(SaveTask);
            return GetTasks(taskTypeId, timeDelta);
        }
    }
}