using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Domain.Models.Entities;

namespace Samba.Persistance.DaoClasses
{
    public interface IEntityDao
    {
        void UpdateEntityScreenItems(EntityScreen entityScreen, int pageNo);
        IEnumerable<Entity> GetEntitiesByState(string state, int entityTypeId);
        List<Entity> FindEntities(EntityType entityType, string searchString, string stateFilter);
        void UpdateEntityState(int entityId, string stateName, string state);
        Entity GetEntityById(int id);
    }
}
