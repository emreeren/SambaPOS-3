using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;

namespace Samba.Infrastructure.Data.SQL
{
    public class EFWorkspace : IWorkspace
    {
        private readonly CommonDbContext _context;

        public EFWorkspace(CommonDbContext context)
        {
            _context = context;
            if (_context.Database.Connection.ConnectionString.EndsWith(".sdf"))
                _context.ObjContext().Connection.Open();
        }

        public void CommitChanges()
        {
            _context.SaveChanges();
        }

        public void ResetDatabase()
        {
            // do nothing.
        }

        public void MarkUnchanged2(IEntityClass item)
        {
            _context.Entry(item).State = EntityState.Unchanged;
        }

        public void MarkUnchanged<T>(T item) where T : class, IEntityClass
        {
            var entity = _context.ChangeTracker.Entries<T>().SingleOrDefault(x => x.Entity.Id == item.Id);
            MarkUnchanged2(entity != null ? entity.Entity : item);
        }

        public void Refresh(object item)
        {
            _context.Refresh(item);
        }

        public void Delete<T>(Expression<Func<T, bool>> expression) where T : class
        {
            foreach (var item in _context.Set<T>().Where(expression))
                Delete(item);
        }

        public void Delete<T>(T item) where T : class
        {
            _context.Set<T>().Remove(item);
        }

        public T Single<T>(Expression<Func<T, bool>> expression, params Expression<Func<T, object>>[] includes) where T : class
        {
            if (includes == null || includes.Length < 1)
                return _context.Trackable<T>().Where(expression).SingleOrDefault();
            var result = includes.Aggregate(_context.Trackable<T>(), (current, include) => current.Include(include)).Where(expression);
            return result.SingleOrDefault();
        }

        public T Last<T>() where T : class,IValueClass
        {
            return _context.Set<T>().OrderByDescending(x => x.Id).Take(1).FirstOrDefault();
        }

        public T Last<T>(Expression<Func<T, bool>> expression) where T : class, IValueClass
        {
            return _context.Set<T>().Where(expression).OrderByDescending(x => x.Id).Take(1).FirstOrDefault();
        }

        public IEnumerable<T> All<T>(params Expression<Func<T, object>>[] includes) where T : class
        {
            return includes.Aggregate(_context.Trackable<T>(), (current, include) => current.Include(include));
        }

        //public IEnumerable<T> All<T>(Expression<Func<T, bool>> expression) where T : class
        //{
        //    return _context.Set<T>().Where(expression);
        //}

        public IEnumerable<T> All<T>(Expression<Func<T, bool>> expression, params Expression<Func<T, object>>[] includes) where T : class
        {
            if (includes == null || includes.Length < 1)
                return _context.Trackable<T>().Where(expression);
            return includes.Aggregate(_context.Trackable<T>(), (current, include) => current.Include(include)).Where(expression);
        }

        public IEnumerable<T> Query<T>(int limit = 0) where T : class
        {
            if (limit == 0) return All<T>();
            return _context.Set<T>().Take(limit);
        }

        public IEnumerable<T> Query<T>(Expression<Func<T, bool>> expression, int limit = 0) where T : class
        {
            if (limit == 0) return All(expression);
            return _context.Set<T>().Where(expression).Take(limit);
        }

        public void Add<T>(T item) where T : class
        {
            _context.Set<T>().Add(item);
        }

        public void Add<T>(IEnumerable<T> items) where T : class
        {
            foreach (var item in items)
                Add(item);
        }

        public void Update<T>(T item) where T : class
        {
            var idf = item as IEntityClass;
            if (idf != null && idf.Id == 0)
                Add(item);
        }

        public int Count<T>() where T : class
        {
            return _context.Set<T>().Count();
        }

        public void ReloadAll()
        {

        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
