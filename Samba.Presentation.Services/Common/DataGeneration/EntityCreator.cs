using System.Collections.Generic;
using System.Linq;
using Samba.Domain.Models.Entities;
using Samba.Infrastructure.Data;
using Samba.Persistance.Data;

namespace Samba.Presentation.Services.Common.DataGeneration
{
    public static class EntityCreator
    {
        public static IEnumerable<Entity> ImportText(string[] values, IWorkspace workspace)
        {
            return ImportPlainText(values, workspace);
        }

        private static IEnumerable<Entity> ImportPlainText(string[] values, IWorkspace workspace)
        {
            IList<Entity> result = new List<Entity>();
            if (values.Length > 0)
            {
                var entityTypes = workspace.All<EntityType>().ToList();
                EntityType currentEntityType = null;

                foreach (var value in values)
                {
                    if (value.StartsWith("#"))
                    {
                        currentEntityType = CreateEntityType(value, entityTypes);
                    }
                    else if (currentEntityType != null)
                    {
                        var entity = CreateEntity(workspace, value, currentEntityType);
                        if (entity != null)
                        {
                            result.Add(entity);
                        }
                    }
                }
            }
            return result;
        }

        private static Entity CreateEntity(IWorkspace workspace, string name, EntityType currentEntityType)
        {
            var entityName = name.ToLower().Trim();
            return workspace.Single<Entity>(x => x.Name.ToLower() == entityName) != null
                ? null
                : new Entity { Name = name, EntityTypeId = currentEntityType.Id };
        }

        private static EntityType CreateEntityType(string item, IEnumerable<EntityType> entityTypes)
        {
            var entityTypeName = item.Trim('#', ' ');
            var entityType = entityTypes.SingleOrDefault(x => x.Name.ToLower() == entityTypeName.ToLower());
            if (entityType == null)
            {
                using (var w = WorkspaceFactory.Create())
                {
                    entityType = new EntityType { Name = entityTypeName };
                    w.Add(entityType);
                    w.CommitChanges();
                }
            }
            return entityType;
        }
    }
}