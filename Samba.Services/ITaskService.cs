using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Domain.Models.Tasks;

namespace Samba.Services
{
    public interface ITaskService
    {
        IEnumerable<Task> GetTasks();
        Task AddNewTask(string taskContent);
    }
}
