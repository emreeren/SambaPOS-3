using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Domain.Models.Tasks;

namespace Samba.Persistance.DaoClasses
{
    public interface ITaskDao
    {
        void SaveTask(Task task);
        IEnumerable<Task> GetTasks(int taskTypeId, DateTime endDate);
    }
}
