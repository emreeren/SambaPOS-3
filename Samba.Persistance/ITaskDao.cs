using System;
using System.Collections.Generic;
using Samba.Domain.Models.Tasks;

namespace Samba.Persistance
{
    public interface ITaskDao
    {
        void SaveTask(Task task);
        IEnumerable<Task> GetTasks(int taskTypeId, DateTime endDate);
    }
}
