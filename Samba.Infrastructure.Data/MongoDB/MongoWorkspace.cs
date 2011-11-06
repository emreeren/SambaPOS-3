using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Norm;

namespace Samba.Infrastructure.Data.MongoDB
{
    public class MongoWorkspace : IWorkspace, IReadOnlyWorkspace
    {
        private readonly IMongo _provider;

        public MongoWorkspace(string db)
        {
            //File.Exists("C:\\Data\\db\\" + db + ".ns");
            _provider = Mongo.Create(db);
        }

        public void Dispose()
        {
            _provider.Dispose();
        }

        public void CommitChanges()
        {
            //throw new NotImplementedException();
        }

        public void Delete<T>(Expression<Func<T, bool>> expression) where T : class
        {
            var items = All(expression);
            foreach (var item in items)
            {
                Delete(item);
            }
        }

        public void Delete<T>(T item) where T : class
        {
            _provider.GetCollection<T>().Delete(item);
        }

        public void DeleteAll<T>() where T : class
        {
            _provider.Database.DropCollection(typeof(T).Name);
        }

        public T Single<T>(Expression<Func<T, bool>> expression) where T : class
        {
            return _provider.GetCollection<T>().AsQueryable().Where(expression).SingleOrDefault();
        }

        public T Last<T>() where T : class, IEntity
        {
            return _provider.GetCollection<T>().AsQueryable().LastOrDefault();
        }

        public IEnumerable<T> All<T>() where T : class
        {
            return _provider.GetCollection<T>().AsQueryable();
        }

        public IEnumerable<T> All<T>(params Expression<Func<T, object>>[] includes) where T : class
        {
            return _provider.GetCollection<T>().AsQueryable();
        }

        public IEnumerable<T> All<T>(Expression<Func<T, bool>> expression) where T : class
        {
            return _provider.GetCollection<T>().AsQueryable().Where(expression);
        }

        public void Add<T>(T item) where T : class
        {
            var idt = item as IEntity;
            if (idt != null)
            {
                if (idt.Id == 0) idt.Id = Convert.ToInt32(_provider.GetCollection<T>().GenerateId());
            }
            _provider.GetCollection<T>().Insert(item);
        }

        public void Add<T>(IEnumerable<T> items) where T : class
        {
            foreach (var item in items)
            {
                Add(item);
            }
        }

        public void Update<T>(T item) where T : class
        {
            //Guid id = ((dynamic)item).Id;
            //_provider.GetCollection<T>().UpdateOne(new { _id = id }, item);
            var idf = item as IEntity;
            if (idf != null && idf.Id == 0) Add(item);
        }

        public int Count<T>() where T : class
        {
            return Convert.ToInt32(_provider.GetCollection<T>().Count());
        }

        public void ReloadAll()
        {
            //throw new NotImplementedException();
        }

        public void ResetDatabase()
        {
            //throw new NotImplementedException();
        }

        public void Refresh(IEnumerable collection)
        {
            //throw new NotImplementedException();
        }

        public void Refresh(object item)
        {
            //throw new NotImplementedException();
        }

        public void Refresh(object item, string property)
        {
            //throw new NotImplementedException();
        }

        public IEnumerable<T> Query<T>(Expression<Func<T, bool>> predictate, params Expression<Func<T, object>>[] includes) where T : class
        {
            //return All(predictate);
            if (predictate != null)
                return All(predictate);
            return All<T>();
        }

        public IEnumerable<string> Distinct<T>(Expression<Func<T, string>> expression) where T : class
        {
            return _provider.GetCollection<T>().AsQueryable().Select(expression).Distinct().Where(x => !string.IsNullOrEmpty(x)).ToList();
        }

        public TResult Single<TSource, TResult>(int id, Expression<Func<TSource, TResult>> expression) where TSource : class, IEntity
        {
            return _provider.GetCollection<TSource>().AsQueryable().Where(x => x.Id == id).Select(expression).SingleOrDefault();
        }

        public T Single<T>(Expression<Func<T, bool>> predictate, string[] includes) where T : class
        {
            return Single(predictate);
        }

        public T Single<T>(Expression<Func<T, bool>> predictate, params Expression<Func<T, object>>[] includes) where T : class
        {
            return Single(predictate);
        }

        public IEnumerable<TResult> Select<TSource, TResult>(Expression<Func<TSource, TResult>> expression, Expression<Func<TSource, bool>> predictate) where TSource : class
        {
            if (predictate != null)
                return _provider.GetCollection<TSource>().AsQueryable().Where(predictate).Select(expression);
            return _provider.GetCollection<TSource>().AsQueryable().Select(expression);
        }

        public int Count<T>(Expression<Func<T, bool>> predictate) where T : class
        {
            if (predictate != null)
                return _provider.GetCollection<T>().AsQueryable().Count(predictate);
            return _provider.GetCollection<T>().AsQueryable().Count();
        }

        public decimal Sum<T>(Expression<Func<T, decimal>> selector, Expression<Func<T, bool>> predictate) where T : class
        {
            if (predictate != null)
                return _provider.GetCollection<T>().AsQueryable().Where(predictate).Sum(selector);
            return _provider.GetCollection<T>().AsQueryable().Sum(selector.Compile());
        }

        public T Last<T>(Expression<Func<T, bool>> predictate, Expression<Func<T, object>>[] includes) where T : class, IEntity
        {
            return _provider.GetCollection<T>().AsQueryable().Last(predictate);
        }

        public IEnumerable<T> Last<T>(int recordCount) where T : class,IEntity
        {
            var coll = _provider.GetCollection<T>().AsQueryable();
            var count = coll.Count();
            if (count > recordCount)
                return coll.Skip(count - recordCount).Take(recordCount);
            return coll;
        }

        public IQueryable<T> Queryable<T>() where T : class
        {
            return _provider.GetCollection<T>().AsQueryable();
        }
    }
}
