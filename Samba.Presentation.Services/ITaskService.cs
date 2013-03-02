﻿using System.Collections.Generic;
using Samba.Domain.Models.Tasks;

namespace Samba.Presentation.Services
{
    public interface ITaskService
    {
        Task AddNewTask(int taskTypeId, string taskContent);
        IEnumerable<Task> GetTasks(int taskTypeId, int timeDelta = 0);
        IEnumerable<Task> SaveTasks(int taskTypeId, IEnumerable<Task> tasks, int timeDelta = 0);
    }
}
