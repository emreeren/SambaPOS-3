using System;
using System.Collections.Generic;
using System.Data.Objects;
using System.Linq;
using System.Data.Entity;
using System.Linq.Expressions;

namespace Samba.Infrastructure.Data.SQL
{
    public class ReadOnlyEFWorkspace : IReadOnlyWorkspace
    {
        private readonly CommonDbContext _context;

        public ReadOnlyEFWorkspace(CommonDbContext context)
        {
            _context = context;
        }

        public IEnumerable<T> Query<T>(Expression<Func<T, bool>> predictate, params Expression<Func<T, object>>[] includes) where T : class
        {
            if (includes != null && predictate != null)
                return includes.Aggregate(_context.ReadOnly<T>(), (current, include) => current.Include(include)).Where(predictate);
            if (includes != null)
                return includes.Aggregate(_context.ReadOnly<T>(), (current, include) => current.Include(include));
            if (predictate != null)
                return _context.ReadOnly<T>().Where(predictate);
            return _context.ReadOnly<T>();
        }

        public IEnumerable<string> Distinct<T>(Expression<Func<T, string>> expression) where T : class
        {
            return _context.ReadOnly<T>().Select(expression).Distinct().Where(x => !string.IsNullOrEmpty(x)).ToList();
        }

        public T Single<T>(Expression<Func<T, bool>> expression, string[] includes) where T : class
        {
            if (includes == null || includes.Length < 1)
                return _context.ReadOnly<T>().Where(expression).SingleOrDefault();
            return includes.Aggregate(_context.ReadOnly<T>() as ObjectQuery<T>,
                                      (current, include) => current.Include(include)).Where(expression).SingleOrDefault();
        }

        public T Single<T>(Expression<Func<T, bool>> predictate, params Expression<Func<T, object>>[] includes) where T : class
        {
            return includes.Aggregate(_context.ReadOnly<T>(), (current, include) => current.Include(include)).Where(predictate).SingleOrDefault();
        }

        public IEnumerable<TResult> Select<TSource, TResult>(Expression<Func<TSource, TResult>> expression, Expression<Func<TSource, bool>> predictate) where TSource : class
        {
            if (predictate != null)
                return _context.ReadOnly<TSource>().Where(predictate).Select(expression);
            return _context.ReadOnly<TSource>().Select(expression);
        }

        public int Count<T>(Expression<Func<T, bool>> predictate) where T : class
        {
            if (predictate != null)
                return _context.ReadOnly<T>().Count(predictate);
            return _context.ReadOnly<T>().Count();
        }

        public decimal Sum<T>(Expression<Func<T, decimal>> selector, Expression<Func<T, bool>> predictate) where T : class
        {
            try
            {
                if (predictate != null)
                    return _context.ReadOnly<T>().Where(predictate).Sum(selector);
                return _context.ReadOnly<T>().Sum(selector);
            }
            catch (InvalidOperationException)
            {
                return 0;
            }
        }

        public T Last<T>(Expression<Func<T, bool>> predictate, Expression<Func<T, object>>[] includes) where T : class,IEntity
        {
            return includes.Aggregate(_context.ReadOnly<T>(), (current, include) => current.Include(include))
                .Where(predictate)
                .OrderByDescending(x => x.Id)
                .Take(1)
                .FirstOrDefault();
        }

        public IEnumerable<T> Last<T>(int recordCount) where T : class,IEntity
        {
            return _context.ReadOnly<T>()
                .OrderByDescending(x => x.Id)
                .Take(recordCount)
                .ToList();
        }

        public TResult Single<TSource, TResult>(int id, Expression<Func<TSource, TResult>> expression) where TSource : class ,IEntity
        {
            return _context.ReadOnly<TSource>().Where(x => x.Id == id).Select(expression).SingleOrDefault();
        }

        public IQueryable<T> Queryable<T>() where T : class
        {
            return _context.ReadOnly<T>();
        }

        public void Dispose()
        {
            _context.Close();
            _context.Dispose();
        }
    }
}