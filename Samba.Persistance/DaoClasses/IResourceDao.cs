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
        IEnumerable<Resource> GetResourcesByState(string state, int resourceTypeId);
        List<Resource> FindResources(ResourceType resourceType, string searchString, string stateFilter);
        void UpdateResourceState(int resourceId, string stateName, string state);
        Resource GetResourceById(int id);
    }
}
