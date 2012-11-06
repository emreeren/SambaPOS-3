using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Tasks
{
    public class TaskResource : Value
    {
        public int TaskId { get; set; }
        public int ResourceId { get; set; }
        public string ResourceName { get; set; }
    }
}