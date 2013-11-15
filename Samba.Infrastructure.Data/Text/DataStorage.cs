using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Samba.Infrastructure.Data.Text
{
    public class DataStorage
    {
        public Dictionary<string, Dictionary<int, object>> Items { get; set; }
        public Dictionary<string, int> Identities { get; set; }

        public DataStorage()
        {
            Items = new Dictionary<string, Dictionary<int, object>>();
            Identities = new Dictionary<string, int>();
        }

        internal bool ContainsKey(Type key)
        {
            return Items.ContainsKey(key.FullName);
        }

        internal Dictionary<int, object> GetDataList(Type key)
        {
            if (!Items.ContainsKey(key.FullName))
            {
                var subList = new Dictionary<int, object>();
                Items.Add(key.FullName, subList);
            }
            return Items[key.FullName];
        }

        internal void Clear()
        {
            Items.Clear();
            Identities.Clear();
        }

        internal T GetById<T>(int id)
        {
            Dictionary<int, object> list = GetDataList(typeof(T));
            if (list.ContainsKey(id)) return (T)list[id];
            return default(T);
        }

        internal IEnumerable<T> GetItems<T>()
        {
            Dictionary<int, object> list = GetDataList(typeof(T));
            return list.Values.Cast<T>();
        }

        internal void Delete<T>(int id)
        {
            Dictionary<int, object> list = GetDataList(typeof(T));
            list.Remove(id);
        }

        internal int CreateIdNumber(Type type)
        {
            lock (Identities)
            {
                if (!Identities.ContainsKey(type.FullName))
                    Identities.Add(type.FullName, 0);
                Identities[type.FullName]++;
                return Identities[type.FullName];
            }
        }

        internal void Add(object o)
        {
            var list = GetDataList(o.GetType());
            var idt = o as IValueClass;
            if (idt != null)
            {
                if (idt.Id == 0) idt.Id = CreateIdNumber(idt.GetType());
                if (list.ContainsKey(idt.Id))
                    Update(idt);
                else list.Add(idt.Id, idt);
            }
            else
            {
                if (list.ContainsKey(0)) Update(o);
                else list.Add(0, o);
            }
        }

        internal void Update(object o)
        {
            var list = GetDataList(o.GetType());

            var idt = o as IValueClass;

            if (idt != null)
            {
                if (idt.Id == 0)
                {
                    Add(idt);
                    return;
                }
                if (list.ContainsKey(idt.Id))
                    list[idt.Id] = idt;
            }
            else if (list.ContainsKey(0))
            {
                list[0] = o;
            }
            else list.Add(0, o);
        }

        internal IList<T> GetItems<T>(Expression<Func<T, bool>> predicate)
        {
            return GetDataList(typeof(T)).Values.Cast<T>().Where(predicate.Compile()).ToList();
        }



    }
}