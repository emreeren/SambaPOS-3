﻿using System.Collections.Generic;
using Samba.Domain.Models.Entities;

namespace Samba.Persistance
{
    public interface IEntityDao
    {
        void UpdateEntityScreenItems(EntityScreen entityScreen, int pageNo);
        IEnumerable<Entity> GetEntitiesByState(string state, int entityTypeId);
        List<Entity> FindEntities(EntityType entityType, string searchString, string stateFilter);
        List<Entity> FindEntities(EntityType entityType, string fieldName, string searchValue, string stateFilter);
        void UpdateEntityState(int entityId, string stateName, string state);
        Entity GetEntityById(int id);
    }
}
