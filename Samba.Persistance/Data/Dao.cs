using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Samba.Infrastructure.Data;
using Samba.Persistance.Specification;

namespace Samba.Persistance.Data
{
    public static class Dao
    {
        private static readonly IDictionary<Type, ArrayList> Cache = new Dictionary<Type, ArrayList>();

        private static void AddToCache<T>(T item) where T : class,IEntityClass
        {
            if (item == null) return;
            
            if (!Cache.ContainsKey(typeof (T)))
            {
                Cache.Add(typeof(T), new ArrayList());
            }
            Cache[typeof(T)].Add(item);
        }

        private static T GetFromCache<T>(Expression<Func<T, bool>> predictate)
        {
            if (Cache.ContainsKey(typeof(T)))
                return Cache[typeof(T)].Cast<T>().SingleOrDefault(predictate.Compile());
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

        public static TResult Single<TSource, TResult>(int id, Expression<Func<TSource, TResult>> expression) where TSource : class,IEntityClass
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

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static T SingleWithCache<T>(Expression<Func<T, bool>> predictate) where T : class, IEntityClass
        {
            var ci = GetFromCache(predictate);
            if (ci != null) return ci;

            using (var workspace = WorkspaceFactory.CreateReadOnly())
            {
                var result = workspace.Single(predictate);
                AddToCache(result);
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

        public static IEnumerable<string> Distinct<T>(Expression<Func<T, string>> expression, Expression<Func<T, bool>> prediction) where T : class
        {
            using (var workspace = WorkspaceFactory.CreateReadOnly())
            {
                return workspace.Distinct(expression, prediction).ToList();
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
                                                  Expression<Func<TSource, bool>> predictate, params Expression<Func<TSource, object>>[] includes) where TSource : class
        {
            using (var workspace = WorkspaceFactory.CreateReadOnly())
            {
                return workspace.Select(expression, predictate, includes).ToList();
            }
        }

        public static IDictionary<int, T> BuildDictionary<T>() where T : class,IEntityClass
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

        public static bool Exists<T>() where T : class,IValueClass
        {
            using (var workspace = WorkspaceFactory.CreateReadOnly())
            {
                return workspace.Any<T>();
            }
        }

        public static bool Exists<T>(Expression<Func<T, bool>> predictate) where T : class,IValueClass
        {
            using (var workspace = WorkspaceFactory.CreateReadOnly())
            {
                return workspace.Select(x => x.Id, predictate).Any();
            }
        }

        public static bool Exists<T>(ISpecification<T> specification) where T : class,IValueClass
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

        public static T Last<T>(Expression<Func<T, bool>> predictate, params Expression<Func<T, object>>[] includes) where T : class ,IEntityClass
        {
            using (var workspace = WorkspaceFactory.CreateReadOnly())
            {
                return workspace.Last(predictate, includes);
            }
        }

        public static IEnumerable<T> Last<T>(int recordCount) where T : class,IEntityClass
        {
            using (var workspace = WorkspaceFactory.CreateReadOnly())
            {
                return workspace.Last<T>(recordCount).OrderBy(x => x.Id);
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static T Load<T>(int id, params Expression<Func<T, object>>[] includes) where T : class, ICacheable
        {
            return CachedDao.CacheLoad(id, includes);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void Save<T>(T entity) where T : class, ICacheable
        {
            CachedDao.CacheSave(entity);
        }

        public static void RemoveFromCache(ICacheable cacheable)
        {
            CachedDao.RemoveFromCache(cacheable);
        }

        public static string CheckConcurrency<T>(T entity) where T : class, ICacheable
        {
            return CachedDao.CheckConcurrency(entity);
        }
    }
}
