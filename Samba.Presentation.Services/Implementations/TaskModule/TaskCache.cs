using System.Collections.Generic;
using Samba.Domain.Models.Tasks;

namespace Samba.Presentation.Services.Implementations.TaskModule
{
    class TaskCache
    {
        public IEnumerable<Task> Tasks { get; set; }
        public TaskCache()
        {
            Tasks = new List<Task>();
        }
    }
}