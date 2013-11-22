using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using Samba.Domain.Models.Entities;
using Samba.Domain.Models.Tickets;
using Samba.Infrastructure.Data;
using Samba.Infrastructure.Data.Validation;
using Samba.Localization.Properties;
using Samba.Persistance.Data;
using Samba.Persistance.Specification;

namespace Samba.Persistance.Implementations
{
    class EntityDeleteValidator : SpecificationValidator<Entity>
    {
        public override string GetErrorMessage(Entity model)
        {
            if (Dao.Exists<EntityScreenItem>(y => y.EntityId == model.Id))
                return string.Format(Resources.DeleteErrorUsedBy_f, Resources.Entity, Resources.EntityScreen);
            if (Dao.Exists<TicketEntity>(y => y.EntityId == model.Id))
                return string.Format(Resources.DeleteErrorUsedBy_f, Resources.Entity, Resources.Ticket);
            return "";
        }
    }

    [Export(typeof(IEntityDao))]
    class EntityDao : IEntityDao
    {
        [ImportingConstructor]
        public EntityDao()
        {
            ValidatorRegistry.RegisterDeleteValidator(new EntityDeleteValidator());
            ValidatorRegistry.RegisterDeleteValidator<EntityType>(x => Dao.Exists<Entity>(y => y.EntityTypeId == x.Id), Resources.EntityType, Resources.Entity);
            ValidatorRegistry.RegisterSaveValidator(new NonDuplicateSaveValidator<Entity>(string.Format(Resources.SaveErrorDuplicateItemName_f, Resources.Entity)));
            ValidatorRegistry.RegisterSaveValidator(new NonDuplicateSaveValidator<EntityType>(string.Format(Resources.SaveErrorDuplicateItemName_f, Resources.EntityType)));
            ValidatorRegistry.RegisterSaveValidator(new NonDuplicateSaveValidator<EntityScreenItem>(string.Format(Resources.SaveErrorDuplicateItemName_f, Resources.EntityScreenItem)));
        }

        private readonly IList<Entity> _emptyEntityList = new List<Entity>().AsReadOnly();

        public void UpdateEntityScreenItems(EntityScreen entityScreen, int pageNo)
        {
            if (entityScreen == null) return;

            IEnumerable<int> set;
            if (entityScreen.PageCount > 1)
            {
                set = entityScreen.ScreenItems
                    .OrderBy(x => x.SortOrder)
                    .Skip(pageNo * entityScreen.ItemCountPerPage)
                    .Take(entityScreen.ItemCountPerPage)
                    .Select(x => x.EntityId);
            }
            else set = entityScreen.ScreenItems.OrderBy(x => x.SortOrder).Select(x => x.EntityId);
            using (var w = WorkspaceFactory.CreateReadOnly())
            {
                var result = w.Queryable<EntityStateValue>().Where(x => set.Contains(x.EntityId));
                result.ToList().ForEach(x =>
                {
                    var screeenItem = entityScreen.ScreenItems.Single(y => y.EntityId == x.EntityId);
                    screeenItem.EntityState = x.GetStateValue(entityScreen.DisplayState);
                });
            }
        }

        public IEnumerable<Entity> GetEntitiesByState(string state, int entityTypeId)
        {
            var sv = string.Format("\"S\":\"{0}\"", state);
            using (var w = WorkspaceFactory.CreateReadOnly())
            {
                var ids = w.Queryable<EntityStateValue>().GroupBy(x => x.EntityId).Select(x => x.Max(y => y.Id));
                var vids = w.Queryable<EntityStateValue>().Where(x => ids.Contains(x.Id) && (x.EntityStates.Contains(sv))).Select(x => x.EntityId).ToList();
                if (vids.Count > 0)
                    return w.Queryable<Entity>().Where(x => x.EntityTypeId == entityTypeId && vids.Contains(x.Id)).ToList();
                return _emptyEntityList;
            }
        }

        public List<Entity> FindEntities(EntityType entityType, string searchString, string stateFilter)
        {
            var templateId = entityType != null ? entityType.Id : 0;
            var searchValue = searchString.ToLower();

            using (var w = WorkspaceFactory.CreateReadOnly())
            {
                var result =
                    w.Query<Entity>(
                        x => x.EntityTypeId == templateId &&
                        (x.CustomData.Contains(searchString) || x.Name.Contains(searchString) || x.Name.ToLower().Contains(searchValue))).Take(250).ToList();

                if (entityType != null)
                    result = result.Where(x => entityType.GetMatchingFields(x, searchString).Any(y => !y.Hidden) || x.Name.ToLower().Contains(searchValue)).ToList();

                if (!string.IsNullOrEmpty(stateFilter))
                {
                    var sv = string.Format("\"S\":\"{0}\"", stateFilter);
                    var set = result.Select(x => x.Id).ToList();
                    var ids = w.Queryable<EntityStateValue>().Where(x => set.Contains(x.EntityId) && x.EntityStates.Contains(sv)).GroupBy(x => x.EntityId).Select(x => x.Max(y => y.Id));
                    var entityIds = w.Queryable<EntityStateValue>().Where(x => ids.Contains(x.Id)).Select(x => x.EntityId).ToList();
                    result = result.Where(x => entityIds.Contains(x.Id)).ToList();
                }
                return result;
            }
        }

        public List<Entity> FindEntities(EntityType entityType, string fieldName, string searchValue, string stateFilter)
        {
            var result = FindEntities(entityType, searchValue, stateFilter);
            return result.Where(x => x.GetCustomData(fieldName).Contains(searchValue)).ToList();
        }

        public void UpdateEntityData(int entityId, string fieldName, string value)
        {
            if (entityId == 0) return;
            using (var w = WorkspaceFactory.Create())
            {
                var entity = w.Single<Entity>(x => x.Id == entityId);
                if (entity != null)
                {
                    entity.SetCustomData(fieldName, value);
                    w.CommitChanges();
                }
            }
        }

        public void UpdateEntityData(EntityType entityType, string entityName, string fieldName, string value)
        {
            using (var w = WorkspaceFactory.Create())
            {
                var entity = w.Single<Entity>(x => x.Name == entityName && x.EntityTypeId == entityType.Id);
                if (entity != null)
                {
                    entity.SetCustomData(fieldName, value);
                    w.CommitChanges();
                }
            }
        }

        public Entity GetEntityByName(string entityName, int entityTypeId)
        {
            return Dao.Single<Entity>(x => x.Name == entityName && x.EntityTypeId == entityTypeId);
        }

        public EntityType GetEntityTypeById(int entityTypeId)
        {
            return Dao.Single<EntityType>(x => x.Id == entityTypeId);
        }

        public EntityStateValue UpdateEntityState(int entityId, string stateName, string state, string quantityExp)
        {
            return UpdateEntityStateEH(entityId, stateName, state, quantityExp, 0);
        }

        private EntityStateValue UpdateEntityStateEH(int entityId, string stateName, string state, string quantityExp, int retries)
        {
            if (entityId == 0) return null;
            using (var w = WorkspaceFactory.Create())
            {
                var stateValue = w.Single<EntityStateValue>(x => x.EntityId == entityId);
                if (stateValue == null)
                {
                    stateValue = new EntityStateValue { EntityId = entityId };
                    w.Add(stateValue);
                }
                stateValue.SetStateValue(stateName, state, quantityExp);
                try
                {
                    w.CommitChanges();
                }
                catch (DbUpdateException)
                {
                    if (retries > 3) throw;
                    return UpdateEntityStateEH(entityId, stateName, state, quantityExp, ++retries);
                }
                return stateValue;
            }
        }

        public Entity GetEntityById(int id)
        {
            return Dao.Single<Entity>(x => x.Id == id);
        }

        public void SaveEntity(Entity entity)
        {
            Dao.Save(entity);
        }
    }
}
