using System;
using System.Reflection;

namespace Samba.Infrastructure.Data.BinarySerializer
{
    public class GetSetGeneric<T, TR> : GetSet
    {
        public delegate TR GetValue(T obj);
        public delegate void SetValue(T obj, TR value);
        private readonly GetValue _get;
        private readonly SetValue _set;

        public GetSetGeneric(PropertyInfo info)
        {
            Name = info.Name;
            Info = info;
            CollectionType = Info.PropertyType.GetInterface("IEnumerable", true) != null;
            var getMethod = info.GetGetMethod();
            var setMethod = info.GetSetMethod();
            _get = (GetValue)Delegate.CreateDelegate(typeof(GetValue), getMethod);
            if (setMethod != null) _set = (SetValue)Delegate.CreateDelegate(typeof(SetValue), setMethod);
        }

        public GetSetGeneric(FieldInfo info)
        {
            Name = info.Name;
            FieldInfo = info;
            _get = new GetValue(GetFieldValue);
            _set = new SetValue(SetFieldValue);
            CollectionType = FieldInfo.FieldType.GetInterface("IEnumerable", true) != null;
            return;
        }


        public GetSetGeneric(string name)
        {
            Name = name;
            var t = typeof(T);
            var p = t.GetProperty(name);
            if (p == null)
            {
                FieldInfo = typeof(T).GetField(Name);
                _get = new GetValue(GetFieldValue);
                _set = new SetValue(SetFieldValue);
                CollectionType = FieldInfo.FieldType.GetInterface("IEnumerable", true) != null;
                return;
            }
            Info = p;
            CollectionType = Info.PropertyType.GetInterface("IEnumerable", true) != null;
            var getMethod = p.GetGetMethod();
            var setMethod = p.GetSetMethod();
            _get = (GetValue)Delegate.CreateDelegate(typeof(GetValue), getMethod);
            if (setMethod != null) _set = (SetValue)Delegate.CreateDelegate(typeof(SetValue), setMethod);
        }

        private TR GetFieldValue(T obj)
        {
            return (TR)FieldInfo.GetValue(obj);
        }

        private void SetFieldValue(T obj, TR value)
        {
            FieldInfo.SetValue(obj, value);
        }


        public override object Get(object item)
        {
            return _get((T)item);
        }

        public override void Set(object item, object value)
        {
            _set((T)item, (TR)value);
        }
    }


}