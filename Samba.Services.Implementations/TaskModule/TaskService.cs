using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Samba.Domain.Models.Tasks;
using Samba.Persistance.Data;

namespace Samba.Services.Implementations.TaskModule
{
    [Export(typeof(ITaskService))]
    public class TaskService : ITaskService
    {
        public IEnumerable<Task> GetTasks()
        {
            return Dao.Query<Task>();
        }

        public Task AddNewTask(string taskContent)
        {
            using (var w = WorkspaceFactory.Create())
            {
                var task = new Task { Content = taskContent };
                w.Add(task);
                w.CommitChanges();
                return task;
            }
        }
    }
}