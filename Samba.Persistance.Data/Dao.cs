using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Omu.ValueInjecter;
using Samba.Infrastructure.Data;
using Samba.Infrastructure.Data.Serializer;

namespace Samba.Persistance.Data
{
    public static class Dao
    {
        private static readonly IDictionary<Type, IDictionary<int, ArrayList>> Cache = new Dictionary<Type, IDictionary<int, ArrayList>>();

        private static void AddToCache(Type t, int key, object item)
        {
            if (item == null) return;
            if (!Cache.ContainsKey(t))
                Cache.Add(t, new Dictionary<int, ArrayList>());
            if (!Cache[t].ContainsKey(key))
                Cache[t].Add(key, new ArrayList());
            Cache[t][key].Add(item);
        }

        private static T GetFromCache<T>(Expression<Func<T, bool>> predictate, int key)
        {
            if (Cache.ContainsKey(typeof(T)))
                if (Cache[typeof(T)].ContainsKey(key))
                    return Cache[typeof(T)][key].Cast<T>().SingleOrDefault(predictate.Compile());
            return default(T);
        }

        public static void ResetCache()
        {
            foreach (var arrayList in Cache.Values)
            {
                arrayList.Clear();
            }
            Cache.Clear();
        }

        public static TResult Single<TSource, TResult>(int id, Expression<Func<TSource, TResult>> expression) where TSource : class,IEntity
        {
            using (var workspace = WorkspaceFactory.CreateReadOnly())
            {
                return workspace.Single(id, expression);
            }
        }

        public static T Single<T>(Expression<Func<T, bool>> predictate, params Expression<Func<T, object>>[] includes) where T : class
        {
            using (var workspace = WorkspaceFactory.CreateReadOnly())
            {
                var result = workspace.Single(predictate, includes);
                return result;
            }
        }

        public static T SingleWithCache<T>(Expression<Func<T, bool>> predictate, params Expression<Func<T, object>>[] includes) where T : class
        {
            var ci = GetFromCache(predictate, ObjectCloner.DataHash(includes));
            if (ci != null) return ci;

            using (var workspace = WorkspaceFactory.CreateReadOnly())
            {
                var result = workspace.Single(predictate, includes);
                AddToCache(typeof(T), ObjectCloner.DataHash(includes), result);
                return result;
            }
        }

        public static IEnumerable<string> Distinct<T>(Expression<Func<T, string>> expression) where T : class
        {
            using (var workspace = WorkspaceFactory.CreateReadOnly())
            {
                return workspace.Distinct(expression).ToList();
            }
        }

        public static IEnumerable<T> Query<T>(ISpecification<T> specification, params Expression<Func<T, object>>[] includes) where T : class
        {
            return Query(specification.SatisfiedBy());
        }

        public static IEnumerable<T> Query<T>(Expression<Func<T, bool>> predictate, params Expression<Func<T, object>>[] includes) where T : class
        {
            using (var workspace = WorkspaceFactory.CreateReadOnly())
            {
                return workspace.Query(predictate, includes).ToList();
            }
        }

        public static IEnumerable<T> Query<T>(params Expression<Func<T, object>>[] includes) where T : class
        {
            using (var workspace = WorkspaceFactory.CreateReadOnly())
            {
                return workspace.Query(null, includes).ToList();
            }
        }

        public static IEnumerable<TResult> Select<TSource, TResult>(Expression<Func<TSource, TResult>> expression,
                                                  Expression<Func<TSource, bool>> predictate) where TSource : class
        {
            using (var workspace = WorkspaceFactory.CreateReadOnly())
            {
                return workspace.Select(expression, predictate).ToList();
            }
        }

        public static IDictionary<int, T> BuildDictionary<T>() where T : class,IEntity
        {
            IDictionary<int, T> result = new Dictionary<int, T>();
            using (var workspace = WorkspaceFactory.CreateReadOnly())
            {
                var values = workspace.Query<T>(null);
                foreach (var value in values)
                {
                    result.Add(value.Id, value);
                }
            }

            return result;
        }

        public static bool Exists<T>(Expression<Func<T, bool>> predictate) where T : class
        {
            using (var workspace = WorkspaceFactory.CreateReadOnly())
            {
                return workspace.First(predictate) != null;
            }
        }

        public static bool Exists<T>(ISpecification<T> specification) where T : class
        {
            return Exists(specification.SatisfiedBy());
        }

        public static int Count<T>() where T : class
        {
            using (var workspace = WorkspaceFactory.CreateReadOnly())
            {
                return workspace.Count<T>(null);
            }
        }

        public static int Count<T>(Expression<Func<T, bool>> predictate) where T : class
        {
            using (var workspace = WorkspaceFactory.CreateReadOnly())
            {
                return workspace.Count(predictate);
            }
        }

        public static int Count<T>(ISpecification<T> specifiation) where T : class
        {
            return Count(specifiation.SatisfiedBy());
        }

        public static decimal Sum<T>(Expression<Func<T, decimal>> func, Expression<Func<T, bool>> predictate) where T : class
        {
            using (var workspace = WorkspaceFactory.CreateReadOnly())
            {
                return workspace.Sum(func, predictate);
            }
        }

        public static T Last<T>(Expression<Func<T, bool>> predictate, params Expression<Func<T, object>>[] includes) where T : class ,IEntity
        {
            using (var workspace = WorkspaceFactory.CreateReadOnly())
            {
                return workspace.Last(predictate, includes);
            }
        }

        public static IEnumerable<T> Last<T>(int recordCount) where T : class,IEntity
        {
            using (var workspace = WorkspaceFactory.CreateReadOnly())
            {
                return workspace.Last<T>(recordCount).OrderBy(x => x.Id);
            }
        }

        public static void SafeSave<T>(IEnumerable<T> items) where T : class, IEntity
        {
            // Todo: support deleted enitities
            using (var w = WorkspaceFactory.Create())
            {
                var newItems = items.Where(x => x.Id == 0);
                w.Add(newItems);
                
                var set = items.Where(x => x.Id > 0).Select(x => x.Id);
                var dbTickets = w.All<T>(x => set.Contains(x.Id)).ToList();
                dbTickets.ForEach(x => x.InjectFrom<EntityInjection>(items.Single(y => y.Id == x.Id)));
                w.CommitChanges();
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
