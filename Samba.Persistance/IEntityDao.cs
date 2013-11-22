using System.Collections.Generic;
using Samba.Domain.Models.Entities;

namespace Samba.Persistance
{
    public interface IEntityDao
    {
        void UpdateEntityScreenItems(EntityScreen entityScreen, int pageNo);
        IEnumerable<Entity> GetEntitiesByState(string state, int entityTypeId);
        List<Entity> FindEntities(EntityType entityType, string searchString, string stateFilter);
        List<Entity> FindEntities(EntityType entityType, string fieldName, string searchValue, string stateFilter);
        EntityStateValue UpdateEntityState(int entityId, string stateName, string state, string quantityExp);
        Entity GetEntityById(int id);
        void SaveEntity(Entity entity);
        void UpdateEntityData(int entityId, string fieldName, string value);
        void UpdateEntityData(EntityType entityType, string entityName, string fieldName, string value);
        Entity GetEntityByName(string entityName, int entityTypeId);
        EntityType GetEntityTypeById(int entityTypeId);
    }
}
