using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Objects;
using System.Linq;
using System.Text;

namespace Samba.Infrastructure.Data.SQL
{
    public class CommonDbContext : DbContext
    {
        private readonly string _name;

        public CommonDbContext(string name)
            : base(name)
        {
            _name = name;
        }

        public IQueryable<T> ReadOnly<T>() where T : class
        {
            ObjectSet<T> result = ObjContext().CreateObjectSet<T>();
            result.MergeOption = MergeOption.NoTracking;
            return result;
        }

        public IQueryable<T> Trackable<T>() where T : class
        {
            return ObjContext().CreateObjectSet<T>();
        }

        public void Refresh(IEnumerable collection)
        {
            ObjContext().Refresh(RefreshMode.StoreWins, collection);
        }

        public void Refresh(object item)
        {
            ObjContext().Refresh(RefreshMode.StoreWins, item);
        }

        public void AddObject(object item)
        {
            ObjContext().AddObject(_name, item);
        }

        public void Detach(object item)
        {
            ObjContext().Detach(item);
        }

        public void LoadProperty(object item, string propertyName)
        {
            ObjContext().LoadProperty(item, propertyName);
        }

        public void Close()
        {
            ObjContext().Connection.Close();
        }

        public ObjectContext ObjContext()
        {
            return ((IObjectContextAdapter)this).ObjectContext;
        }

        public void AcceptAllChanges()
        {
            ObjContext().AcceptAllChanges();
        }
    }
}
