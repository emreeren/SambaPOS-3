using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Samba.Infrastructure.Data;
using Samba.Infrastructure.Helpers;

namespace Samba.Domain.Models.Tasks
{
    public class Task : EntityClass, ICacheable
    {
        public Task()
        {
            StartDate = DateTime.Now;
            EndDate = StartDate;
            LastUpdateTime = StartDate;
            _taskTokens = new List<TaskToken>();
        }

        public int TaskTypeId { get; set; }
        public string Content { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string CustomData { get; set; }
        public bool Completed { get; set; }

        private IList<TaskToken> _taskTokens;
        public virtual IList<TaskToken> TaskTokens
        {
            get { return _taskTokens; }
            set { _taskTokens = value; }
        }

        public DateTime LastUpdateTime { get; set; }

        private IList<TaskCustomDataValue> _taskCustomDataValues;
        private IList<TaskCustomDataValue> TaskCustomDataValues
        {
            get { return _taskCustomDataValues ?? (_taskCustomDataValues = JsonHelper.Deserialize<List<TaskCustomDataValue>>(CustomData)); }
        }

        public void UpdateCustomDataValue(string fieldName, string value)
        {
            if (TaskCustomDataValues.All(x => x.FieldName != fieldName))
                TaskCustomDataValues.Add(new TaskCustomDataValue { FieldName = fieldName });
            var field = _taskCustomDataValues.First(x => x.FieldName == fieldName);
            field.Value = value;
            if (string.IsNullOrEmpty(value)) _taskCustomDataValues.Remove(field);
            CustomData = JsonHelper.Serialize(TaskCustomDataValues);
            _taskCustomDataValues = null;
        }

        public string GetCustomDataValue(string fieldName)
        {
            var field = TaskCustomDataValues.FirstOrDefault(x => x.FieldName == fieldName);
            return field != null ? field.Value : "";
        }

        public void SetCompleted(bool completed)
        {
            Completed = completed;
            EndDate = completed ? DateTime.Now : StartDate;
        }
    }

    [DataContract]
    public class TaskCustomDataValue
    {
        [DataMember(Name = "N")]
        public string FieldName { get; set; }

        [DataMember(Name = "V", EmitDefaultValue = false)]
        public string Value { get; set; }
    }
}
