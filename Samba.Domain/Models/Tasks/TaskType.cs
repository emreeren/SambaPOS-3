using System.Collections.Generic;
using Samba.Domain.Models.Entities;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Tasks
{
    public class TaskType : EntityClass
    {
        public TaskType()
        {
            _entityTypes = new List<EntityType>();
        }

        private IList<EntityType> _entityTypes;
        public virtual IList<EntityType> EntityTypes
        {
            get { return _entityTypes; }
            set { _entityTypes = value; }
        }
    }
}