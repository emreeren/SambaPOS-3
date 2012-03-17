using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Omu.ValueInjecter;
using Samba.Infrastructure.Data;

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
                    ce.Workspace.CommitChanges();
                    RemoveFromEntityCache<T>(ce);
                    return;
                }
            }

            if (entity.Id == 0)
            {
                var w = WorkspaceFactory.Create();
                AddEntities(entity, w);
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
                    w.CommitChanges();
                }
            }
        }

        public static void SafeSave<T>(T entity) where T : class, IEntity
        {
            using (var w = WorkspaceFactory.Create())
            {
                if (entity.Id == 0)
                {
                    AddEntities(entity, w);
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

        public static void AddEntities(IEntity item, IWorkspace workspace)
        {
            if (item.Id > 0) workspace.MarkUnchanged(item);

            var items = item.GetType().GetProperties()
                 .Where(y => y.CanWrite && y.PropertyType.GetInterfaces().Contains(typeof(IEntity)))
                 .Select(x => x.GetValue(item, null)).Cast<IEntity>().ToList();

            items.ForEach(x => AddEntities(x, workspace));

            var collections =
                item.GetType().GetProperties().Where(
                    x =>
                    x.PropertyType.IsGenericType &&
                    x.PropertyType.GetGenericTypeDefinition().GetInterfaces().Contains(typeof(IEnumerable)) &&
                    x.PropertyType.GetGenericArguments()[0].GetInterfaces().Contains(typeof(IEntity))).ToList();

            var cis = collections.SelectMany(pi => (pi.GetValue(item, null) as IEnumerable).Cast<IEntity>());

            foreach (var i in cis)
            {
                AddEntities(i, workspace);
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
    }


    public class EntityInjection : ConventionInjection
    {
        protected override bool Match(ConventionInfo c)
        {
            var propertyMatch = c.SourceProp.Name == c.TargetProp.Name;
            var sourceNotNull = c.SourceProp.Value != null;

            var targetPropertyIdWritable = true;

            if (propertyMatch && c.TargetProp.Name == "Id" && !(c.Target.Value is IEntity))
                targetPropertyIdWritable = false;

            return propertyMatch && sourceNotNull && targetPropertyIdWritable;
        }

        protected override object SetValue(ConventionInfo c)
        {
            if (c.SourceProp.Type.IsValueType || c.SourceProp.Type == typeof(string))
                return c.SourceProp.Value;

            if (c.SourceProp.Type.IsGenericType)
            {
                var td = c.SourceProp.Type.GetGenericTypeDefinition();
                if (td != null && td.GetInterfaces().Contains(typeof(IEnumerable)))
                {
                    var targetChildType = c.TargetProp.Type.GetGenericArguments()[0];
                    if (targetChildType.IsValueType || targetChildType == typeof(string)) return c.SourceProp.Value;
                    if (targetChildType.GetInterfaces().Any(x => x == typeof(IValue)))
                    {
                        var deleteMethod = c.TargetProp.Value.GetType().GetMethod("Remove");

                        (from vl in (c.TargetProp.Value as IEnumerable).Cast<IValue>()
                         where vl.Id > 0
                         let srcv = (c.SourceProp.Value as IEnumerable).Cast<IValue>().SingleOrDefault(z => z.Id == vl.Id)
                         where srcv == null
                         select vl).ToList().ForEach(x => deleteMethod.Invoke(c.TargetProp.Value, new[] { x }));

                        var sourceCollection = (c.SourceProp.Value as IEnumerable).Cast<IValue>();

                        foreach (var s in sourceCollection)
                        {
                            var sv = s;
                            var target = (c.TargetProp.Value as IEnumerable).Cast<IValue>().SingleOrDefault(z => z.Id == sv.Id && z.Id != 0);
                            if (target != null) target.InjectFrom<EntityInjection>(sv);
                            else
                            {
                                var addMethod = c.TargetProp.Value.GetType().GetMethod("Add");
                                addMethod.Invoke(c.TargetProp.Value, new[] { sv });
                            }
                        }
                    }
                }

                return c.TargetProp.Value;
            }

            if (c.TargetProp.Value == null)
                c.TargetProp.Value = Activator.CreateInstance(c.TargetProp.Type);

            return c.TargetProp.Value.InjectFrom<EntityInjection>(c.SourceProp.Value);
        }
    }

}
