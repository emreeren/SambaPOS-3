using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Tasks
{
    public class TaskToken : ValueClass
    {
        public int TaskId { get; set; }
        public string Caption { get; set; }
        public string Value { get; set; }
        public int Type { get; set; }
        public int ReferenceTypeId { get; set; }
        public int ReferenceId { get; set; }
    }
}