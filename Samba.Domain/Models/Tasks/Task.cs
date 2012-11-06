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
        public bool Completed { get; set; }

        private IList<TaskResource> _taskResources;
        public virtual IList<TaskResource> TaskResources
        {
            get { return _taskResources; }
            set { _taskResources = value; }
        }
    }
}
