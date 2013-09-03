using System.Collections.Generic;
using Samba.Domain.Models.Entities;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Tasks
{
    public class TaskType : EntityClass
    {
        public TaskType()
        {
            _taskCustomFields = new List<TaskCustomField>();
        }

        private IList<TaskCustomField> _taskCustomFields;
        public virtual IList<TaskCustomField> TaskCustomFields
        {
            get { return _taskCustomFields; }
            set { _taskCustomFields = value; }
        }
    }

    public class TaskCustomField : EntityClass
    {
        public int TaskTypeId { get; set; }
        public int FieldType { get; set; }
        public string EditingFormat { get; set; }
        public string DisplayFormat { get; set; }
    }
}