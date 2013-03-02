using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Samba.Infrastructure.Data
{
    public interface IReadOnlyWorkspace : IDisposable
    {
        IEnumerable<T> Query<T>(Expression<Func<T, bool>> predictate, params Expression<Func<T, object>>[] includes) where T : class;
        IEnumerable<string> Distinct<T>(Expression<Func<T, string>> expression) where T : class;
        IEnumerable<string> Distinct<T>(Expression<Func<T, string>> expression, Expression<Func<T, bool>> prediction) where T : class;
        TResult Single<TSource, TResult>(int id, Expression<Func<TSource, TResult>> expression) where TSource : class, IEntityClass;
        T Single<T>(Expression<Func<T, bool>> predictate, params Expression<Func<T, object>>[] includes) where T : class;
        T First<T>(Expression<Func<T, bool>> predictate, params Expression<Func<T, object>>[] includes) where T : class;
        IEnumerable<TResult> Select<TSource, TResult>(Expression<Func<TSource, TResult>> expression, Expression<Func<TSource, bool>> predictate, params Expression<Func<TSource, object>>[] includes) where TSource : class;
        int Count<T>(Expression<Func<T, bool>> predictate) where T : class;
        decimal Sum<T>(Expression<Func<T, decimal>> selector, Expression<Func<T, bool>> predictate) where T : class;
        T Last<T>(Expression<Func<T, bool>> predictate, Expression<Func<T, object>>[] includes) where T : class,IEntityClass;
        IEnumerable<T> Last<T>(int recordCount) where T : class,IEntityClass;
        IQueryable<T> Queryable<T>() where T : class;
        bool Any<T>() where T : class;
    }
}
