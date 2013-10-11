using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Omu.ValueInjecter;
using Samba.Infrastructure.Data;
using Samba.Infrastructure.Data.Injection;
using Samba.Infrastructure.Data.Validation;

namespace Samba.Persistance.Data
{
    public class CacheEntry
    {
        public IWorkspace Workspace { get; set; }
        public ICacheable Cacheable { get; set; }
    }

    internal static class CachedDao
    {
        private static readonly IDictionary<Type, IDictionary<int, CacheEntry>> EntityCache = new Dictionary<Type, IDictionary<int, CacheEntry>>();

        private static void AddToEntityCache<T>(ICacheable entity, IWorkspace workspace)
        {
            if (entity == null) return;
            if (!EntityCache.ContainsKey(typeof(T)))
                EntityCache.Add(typeof(T), new Dictionary<int, CacheEntry>());
            if (EntityCache[typeof(T)].ContainsKey(entity.Id))
            {
                RemoveFromEntityCache<T>(EntityCache[typeof(T)][entity.Id]);
            }
            EntityCache[typeof(T)].Add(entity.Id, new CacheEntry { Cacheable = entity, Workspace = workspace });
        }

        private static CacheEntry GetFromEntitiyCache<T>(int key) where T : class, ICacheable
        {
            if (EntityCache.ContainsKey(typeof(T)))
                if (EntityCache[typeof(T)].ContainsKey(key))
                    return EntityCache[typeof(T)][key];
            return null;
        }

        private static void RemoveFromEntityCache<T>(CacheEntry ce)
        {
            ce.Workspace.Dispose();
            if (EntityCache.ContainsKey(typeof(T)))
                EntityCache[typeof(T)].Remove(ce.Cacheable.Id);
        }

        private static void RemoveFromEntityCache<T>(T entity) where T : ICacheable
        {
            if (EntityCache.ContainsKey(typeof(T)))
            {
                if (EntityCache[typeof(T)].ContainsKey(entity.Id))
                    RemoveFromEntityCache<T>(EntityCache[typeof(T)][entity.Id]);
            }
        }

        private static void RemoveFromEntityCache(ICacheable entity)
        {
            var type = entity.GetRealType();
            if (EntityCache.ContainsKey(type))
            {
                if (EntityCache[type].ContainsKey(entity.Id))
                {
                    var ce = EntityCache[type][entity.Id];
                    ce.Workspace.Dispose();
                    EntityCache[type].Remove(ce.Cacheable.Id);
                }
            }
        }

        private static Type GetRealType(this ICacheable entity)
        {
            var entityType = entity.GetType();
            if (entityType.BaseType != null && entityType.Namespace == "System.Data.Entity.DynamicProxies")
            {
                entityType = entityType.BaseType;
            }
            return entityType;
        }

        public static T CacheLoad<T>(int id, params Expression<Func<T, object>>[] includes) where T : class, ICacheable
        {
            var w = WorkspaceFactory.Create();
            var entity = w.Single(x => x.Id == id, includes);

            AddToEntityCache<T>(entity, w);

            return entity;
        }

        public static void CacheSave<T>(T entity) where T : class, ICacheable
        {
            if (entity.Id > 0)
            {
                var ce = GetFromEntitiyCache<T>(entity.Id);
                if (ce != null)
                {
                    if (ce.Cacheable != entity)
                        ce.Cacheable.InjectFrom<EntityInjection>(entity);
                    entity.LastUpdateTime = DateTime.Now;
                    AddEntities(ce.Cacheable, ce.Workspace, entity.Id);
                    ce.Workspace.CommitChanges();
                    RemoveFromEntityCache<T>(ce);
                    return;
                }
            }

            if (entity.Id == 0)
            {
                var w = WorkspaceFactory.Create();
                AddEntities(entity, w, entity.Id);
                w.Add(entity);
                entity.LastUpdateTime = DateTime.Now;
                w.CommitChanges();
                AddToEntityCache<T>(entity, w);
            }
            else
            {
                using (var w = WorkspaceFactory.Create())
                {
                    var currentItem = w.Single<T>(x => x.Id == entity.Id);
                    currentItem.InjectFrom<EntityInjection>(entity);
                    entity.LastUpdateTime = DateTime.Now;
                    AddEntities(currentItem, w, entity.Id);
                    w.CommitChanges();
                }
            }
        }

        public static void SafeSave<T>(T entity) where T : class, IEntityClass
        {
            using (var w = WorkspaceFactory.Create())
            {
                AddEntities(entity, w, entity.Id);
                if (entity.Id == 0)
                {
                    w.Add(entity);
                }
                else
                {
                    var currentItem = w.Single<T>(x => x.Id == entity.Id);
                    currentItem.InjectFrom<EntityInjection>(entity);
                }
                w.CommitChanges();
            }
        }

        public static void AddEntities<T>(T item, IWorkspace workspace, int parentId) where T : class, IEntityClass
        {
            if (item.Id > 0 && parentId == 0)
                workspace.MarkUnchanged(item);

            var items = item.GetType().GetProperties()
                 .Where(y => y.CanWrite && y.PropertyType.GetInterfaces().Contains(typeof(IEntityClass)))
                 .Select(x => x.GetValue(item, null)).Cast<IEntityClass>().ToList();

            items.ForEach(x => AddEntities(x, workspace, item.Id));

            var collections =
                item.GetType().GetProperties().Where(
                    x =>
                    x.PropertyType.IsGenericType &&
                    x.PropertyType.GetGenericTypeDefinition().GetInterfaces().Contains(typeof(IEnumerable)) &&
                    x.PropertyType.GetGenericArguments()[0].GetInterfaces().Contains(typeof(IEntityClass))).ToList();

            var cis = collections.SelectMany(pi => (pi.GetValue(item, null) as IEnumerable).Cast<IEntityClass>()).ToList();

            foreach (var i in cis)
            {
                AddEntities(i, workspace, item.Id);
            }
        }

        public static string CheckConcurrency<T>(T entity) where T : class, ICacheable
        {
            var lup = Dao.Single<T, DateTime>(entity.Id, x => x.LastUpdateTime);
            if (entity.LastUpdateTime.CompareTo(lup) == 0) return "";

            using (var w = WorkspaceFactory.Create())
            {
                var loaded = w.Single<T>(x => x.Id == entity.Id);
                var cr = ValidatorRegistry.GetConcurrencyErrorMessage(entity, loaded);
                if (cr.SuggestedOperation == SuggestedOperation.Refresh) RemoveFromEntityCache(entity);
                return cr.ErrorMessage;
            }
        }

        public static void RemoveFromCache(ICacheable entity)
        {
            if (entity == null) return;
            RemoveFromEntityCache(entity);
        }
    }
}
