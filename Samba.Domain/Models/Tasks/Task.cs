using System;
using System.Collections.Generic;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Tasks
{
    public class Task : Entity
    {
        public Task()
        {
            StartDate = DateTime.Now;
            EndDate = StartDate;
            _taskResources = new List<TaskResource>();
        }

        public int TaskTypeId { get; set; }
        public string Content { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool Completed
        {
            get { return _completed; }
            set
            {
                _completed = value;
                EndDate = Completed ? DateTime.Now : StartDate;
            }
        }

        private IList<TaskResource> _taskResources;
        private bool _completed;

        public virtual IList<TaskResource> TaskResources
        {
            get { return _taskResources; }
            set { _taskResources = value; }
        }
    }
}
