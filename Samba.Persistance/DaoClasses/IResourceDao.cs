using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Domain.Models.Resources;

namespace Samba.Persistance.DaoClasses
{
    public interface IResourceDao
    {
        void UpdateResourceScreenItems(ResourceScreen resourceScreen, int pageNo);
        IEnumerable<Resource> GetResourcesByState(int resourceStateId, int resourceTypeId);
        List<Resource> FindResources(ResourceType resourceType, string searchString, int stateFilter);
        void UpdateResourceState(int resourceId, int stateId);
    }
}
