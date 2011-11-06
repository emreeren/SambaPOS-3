using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Objects;
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

        public void Refresh(IEnumerable collection)
        {
            _context.Refresh(collection);
        }

        public void Refresh(object item, string property)
        {
            _context.LoadProperty(item, property);
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

        public void DeleteAll<T>() where T : class
        {
            foreach (var item in _context.Set<T>())
                Delete(item);
        }

        public T Single<T>(Expression<Func<T, bool>> expression) where T : class
        {
            return _context.Set<T>().SingleOrDefault(expression);
        }

        public T Single<T>(Expression<Func<T, bool>> expression, params Expression<Func<T, object>>[] includes) where T : class
        {
            if (includes == null || includes.Length < 1)
                return _context.Trackable<T>().Where(expression).SingleOrDefault();
            var result = includes.Aggregate(_context.Trackable<T>(), (current, include) => current.Include(include)).Where(expression);
            return result.SingleOrDefault();
        }

        public T Last<T>() where T : class,IEntity
        {
            return _context.Set<T>().OrderByDescending(x => x.Id).FirstOrDefault();
        }

        public IEnumerable<T> All<T>() where T : class
        {
            return _context.Set<T>().ToList();
        }

        public IEnumerable<T> All<T>(params Expression<Func<T, object>>[] includes) where T : class
        {
            return includes.Aggregate(_context.Trackable<T>(), (current, include) => current.Include(include));
        }

        public IEnumerable<T> All<T>(Expression<Func<T, bool>> expression) where T : class
        {
            return _context.Set<T>().Where(expression);
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
            var idf = item as IEntity;
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
