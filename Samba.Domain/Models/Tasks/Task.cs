using System;
using System.Collections.Generic;
using Samba.Infrastructure.Data;

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

        private bool _completed;
        public bool Completed
        {
            get { return _completed; }
            set
            {
                _completed = value;
                EndDate = Completed ? DateTime.Now : StartDate;
            }
        }

        private IList<TaskToken> _taskTokens;
        public virtual IList<TaskToken> TaskTokens
        {
            get { return _taskTokens; }
            set { _taskTokens = value; }
        }

        public DateTime LastUpdateTime { get; set; }
    }
}
