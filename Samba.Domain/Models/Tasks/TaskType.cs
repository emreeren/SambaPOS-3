using System.Collections.Generic;
using Samba.Domain.Models.Resources;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Tasks
{
    public class TaskType : Entity
    {
        public TaskType()
        {
            _resourceTypes = new List<ResourceType>();
        }

        private IList<ResourceType> _resourceTypes;
        public virtual IList<ResourceType> ResourceTypes
        {
            get { return _resourceTypes; }
            set { _resourceTypes = value; }
        }
    }
}