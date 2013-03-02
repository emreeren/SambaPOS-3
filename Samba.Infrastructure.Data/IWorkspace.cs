using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Samba.Infrastructure.Data
{
    public interface IWorkspace : IDisposable
    {
        void CommitChanges();
        void Delete<T>(Expression<Func<T, bool>> expression) where T : class;
        void Delete<T>(T item) where T : class;
        T Single<T>(Expression<Func<T, bool>> expression, params Expression<Func<T, object>>[] includes) where T : class;
        T Last<T>() where T : class, IValueClass;
        T Last<T>(Expression<Func<T, bool>> expression) where T : class, IValueClass;
        IEnumerable<T> All<T>(params Expression<Func<T, object>>[] includes) where T : class;
        IEnumerable<T> All<T>(Expression<Func<T, bool>> expression, params Expression<Func<T, object>>[] includes) where T : class;

        IEnumerable<T> Query<T>(int limit = 0) where T : class;
        IEnumerable<T> Query<T>(Expression<Func<T, bool>> expression, int limit = 0) where T : class;

        void Add<T>(T item) where T : class;
        void Add<T>(IEnumerable<T> items) where T : class;
        void Update<T>(T item) where T : class;
        int Count<T>() where T : class;

        void ReloadAll();
        void ResetDatabase();

        void Refresh(object item);

        void MarkUnchanged<T>(T item) where T : class, IEntityClass;
    }
}
