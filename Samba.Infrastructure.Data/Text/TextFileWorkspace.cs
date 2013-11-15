using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using Samba.Infrastructure.Data.BinarySerializer;
using Samba.Infrastructure.Settings;

namespace Samba.Infrastructure.Data.Text
{
    public class TextFileWorkspace : IWorkspace, IReadOnlyWorkspace
    {
        private DataStorage _storage = new DataStorage();
        private readonly string _fileName;
        private readonly bool _asyncSave;

        public TextFileWorkspace()
            : this(GetDefaultDbFileName(), false)
        {

        }

        private static string GetDefaultDbFileName()
        {
            return Path.GetDirectoryName(Assembly.GetCallingAssembly().Location) + "\\SambaDB.txt";
        }

        public TextFileWorkspace(string fileName, bool asyncSave)
        {
            _fileName = fileName;
            _asyncSave = asyncSave;
            ReloadAll();
        }

        private int _tNumber;

        [MethodImpl(MethodImplOptions.Synchronized)]
        private void PersistAllAsync(object o)
        {
            LocalSettings.UpdateThreadLanguage();
            if (!string.IsNullOrEmpty(o.ToString()) && (int)o != _tNumber) return;
            IdFixer.FixIdNumbers(_storage.Items, _storage.CreateIdNumber);
            var data = SilverlightSerializer.Serialize(_storage);
            File.WriteAllBytes(_fileName, data);
        }

        public void CommitChanges()
        {
            if (_asyncSave)
            {
                _tNumber = new Random(DateTime.Now.Millisecond).Next();
                ThreadPool.QueueUserWorkItem(PersistAllAsync, _tNumber);
            }
            else
                PersistAllAsync("");
        }

        public void Delete<T>(Expression<Func<T, bool>> expression) where T : class
        {
            foreach (var item in All(expression))
            {
                Delete(item);
            }
        }

        public void Delete<T>(T item) where T : class
        {
            var idf = item as IEntityClass;
            if (idf != null)
                _storage.Delete<T>(idf.Id);
        }

        public T Last<T>() where T : class,IValueClass
        {
            return _storage.GetItems<T>().LastOrDefault();
        }

        public T Last<T>(Expression<Func<T, bool>> expression) where T : class, IValueClass
        {
            return _storage.GetItems<T>().AsQueryable().Where(expression).LastOrDefault();
        }

        public IEnumerable<T> All<T>(params Expression<Func<T, object>>[] includes) where T : class
        {
            return _storage.GetItems<T>();
        }

        public IEnumerable<T> All<T>(Expression<Func<T, bool>> expression, params Expression<Func<T, object>>[] includes) where T : class
        {
            return _storage.GetItems(expression);
        }

        public IEnumerable<T> Query<T>(Expression<Func<T, bool>> expression, int limit = 0, bool orderByDesc = false) where T : class,IValueClass
        {
            var result = All(expression);
            if (orderByDesc) result = result.OrderByDescending(x => x.Id);
            if (limit > 0) result = result.Take(limit);
            return result;
        }

        public IEnumerable<T> Query<T>(int limit = 0, bool orderByDesc = false) where T : class,IValueClass
        {
            var result = All<T>();
            if (orderByDesc) result = result.OrderByDescending(x => x.Id);
            if (limit > 0) result = result.Take(limit);
            return result;
        }

        public void Add<T>(T item) where T : class
        {
            _storage.Add(item);
            AddSubItems(item);
        }

        private void AddSubItems<T>(T item)
        {
            var collections = item.GetType().GetProperties()
                .Where(x => x.PropertyType.IsGenericType && x.PropertyType.GetInterfaces().Contains(typeof(IEnumerable)));
            foreach (var collectionType in collections.Where(x=>x.PropertyType.GetGenericArguments()[0].GetInterfaces().Any(y=>y == typeof(IValueClass)) ))
            {
                var collection = collectionType.GetValue(item, null) as IEnumerable;
                foreach (var collectionItem in collection)
                {
                    Add(collectionItem);
                }
            }
        }

        public void Update<T>(T item) where T : class
        {
            _storage.Update(item);
        }

        public int Count<T>() where T : class
        {
            return _storage.GetItems<T>().Count();
        }

        public void ReloadAll()
        {
            if (File.Exists(_fileName))
            {
                //var helper = new XmlDeserializerHelper();
                //_storage = helper.Deserialize(_fileName) as DataStorage;
                try
                {
                    var data = File.ReadAllBytes(_fileName);
                    _storage = SilverlightSerializer.Deserialize(data) as DataStorage;
                }
                catch (Exception)
                {
                    _storage = new DataStorage();
                }
            }
        }

        public void ResetDatabase()
        {
            _storage.Clear();
            File.Delete(_fileName);
        }

        public void Refresh(IEnumerable collection)
        {
            // gerekmiyor...
        }

        public void Refresh(object item)
        {
            //gerekmiyor...
        }

        public void Refresh(object item, string property)
        {
            //gerekmiyor...
        }

        public void MarkUnchanged<T>(T item) where T : class, IEntityClass
        {
            //gerekmiyor..
        }

        public IDbTransaction BeginTransaction()
        {
            return null;
        }

        public IEnumerable<T> Query<T>(params string[] includes) where T : class
        {
            return All<T>();
        }

        public void Dispose()
        {
            // gerekmiyor...
        }

        public IEnumerable<T> Query<T>(Expression<Func<T, bool>> predictate, params Expression<Func<T, object>>[] includes) where T : class
        {
            if (predictate != null)
                return All(predictate);
            return All<T>();
        }

        public IEnumerable<string> Distinct<T>(Expression<Func<T, string>> expression) where T : class
        {
            return _storage.GetItems<T>().Select(expression.Compile()).Distinct().Where(x => !string.IsNullOrEmpty(x)).ToList();
        }

        public IEnumerable<string> Distinct<T>(Expression<Func<T, string>> expression, Expression<Func<T, bool>> prediction) where T : class
        {
            return _storage.GetItems<T>().Where(prediction.Compile()).Select(expression.Compile()).Distinct().Where(x => !string.IsNullOrEmpty(x)).ToList();
        }

        public TResult Single<TSource, TResult>(int id, Expression<Func<TSource, TResult>> expression) where TSource : class, IEntityClass
        {
            return _storage.GetItems<TSource>().Where(x => x.Id == id).Select(expression.Compile()).SingleOrDefault();
        }

        public T Single<T>(Expression<Func<T, bool>> expression, string[] includes) where T : class
        {
            return Single(expression);
        }

        public T Single<T>(Expression<Func<T, bool>> predictate, params Expression<Func<T, object>>[] includes) where T : class
        {
            return _storage.GetItems(predictate).FirstOrDefault();
        }

        public T First<T>(Expression<Func<T, bool>> predictate, params Expression<Func<T, object>>[] includes) where T : class
        {
            return _storage.GetItems<T>().FirstOrDefault(predictate.Compile());
        }

        public IEnumerable<TResult> Select<TSource, TResult>(Expression<Func<TSource, TResult>> expression, Expression<Func<TSource, bool>> prediction, params Expression<Func<TSource, object>>[] includes) where TSource : class
        {
            if (prediction != null)
                return _storage.GetItems<TSource>().Where(prediction.Compile()).Select(expression.Compile());
            return _storage.GetItems<TSource>().Select(expression.Compile());
        }

        public int Count<T>(Expression<Func<T, bool>> predictate) where T : class
        {
            if (predictate != null)
                return _storage.GetItems<T>().Count(predictate.Compile());
            return _storage.GetItems<T>().Count();
        }

        public decimal Sum<T>(Expression<Func<T, decimal>> selector, Expression<Func<T, bool>> predictate) where T : class
        {
            if (predictate != null)
                return _storage.GetItems<T>().Where(predictate.Compile()).Sum(selector.Compile());
            return _storage.GetItems<T>().Sum(selector.Compile());
        }

        public T Last<T>(Expression<Func<T, bool>> predictate, Expression<Func<T, object>>[] includes) where T : class,IEntityClass
        {
            return _storage.GetItems<T>().LastOrDefault(predictate.Compile());
        }

        public IEnumerable<T> Last<T>(int recordCount) where T : class,IEntityClass
        {
            var coll = _storage.GetItems<T>().AsQueryable();
            var count = coll.Count();
            return count > recordCount ? coll.Skip(count - recordCount).Take(recordCount) : coll;
        }

        public IQueryable<T> Queryable<T>() where T : class
        {
            return _storage.GetItems<T>().AsQueryable();
        }

        public bool Any<T>() where T : class
        {
            return _storage.GetItems<T>().Any();
        }

        public void ExecSql(string sqlString)
        {
            //
        }
    }
}
