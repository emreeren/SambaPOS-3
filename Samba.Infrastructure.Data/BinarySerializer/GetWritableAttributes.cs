using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Samba.Infrastructure.Data.BinarySerializer
{
    public class GetWritableAttributes
    {
        private static readonly Dictionary<RuntimeTypeHandle, GetSet[][][]> PropertyAccess = new Dictionary<RuntimeTypeHandle, GetSet[][][]>();
        /// <summary>
        /// Dictionary of all the used objects to check if properties are different
        /// to those set during construction
        /// </summary>
        private static readonly Dictionary<RuntimeTypeHandle, object> Vanilla = new Dictionary<RuntimeTypeHandle, object>();

        public static Entry[] GetProperties(object obj)
        {
            var type = obj.GetType().TypeHandle;
            var accessors = GetAccessors(type);

            return (from a in accessors[0]
                    let value = a.Get(obj)
                    where value != null && (!(value is ICollection) || ((ICollection)value).Count > 0) && !value.Equals(a.Vanilla)
                    select new Entry()
                               {
                                   PropertyInfo = a.Info,
                                   MustHaveName = true,
                                   Value = value
                               }).ToArray();
        }

        public static Entry[] GetFields(object obj)
        {
            var type = obj.GetType().TypeHandle;
            var accessors = GetAccessors(type);


            return (from a in accessors[1]
                    let value = a.Get(obj)
                    where value != null && (!(value is ICollection) || ((ICollection)value).Count >0) && !value.Equals(a.Vanilla) 
                    select new Entry()
                               {
                                   FieldInfo = a.FieldInfo,
                                   MustHaveName = true,
                                   Value = value
                               }).ToArray();
        }

        private static object GetVanilla(RuntimeTypeHandle type)
        {
            object vanilla;
            lock (Vanilla)
            {
                if (!Vanilla.TryGetValue(type, out vanilla))
                {
                    vanilla = SilverlightSerializer.CreateObject(Type.GetTypeFromHandle(type));
                    Vanilla[type] = vanilla;
                }
            }
            return vanilla;
        }

        private static GetSet[][] GetAccessors(RuntimeTypeHandle type)
        {
            lock (PropertyAccess)
            {
                var index = (SilverlightSerializer.IsChecksum ? 1 : 0) + (SilverlightSerializer.IsChecksum && SilverlightSerializer.IgnoreIds ? 1 : 0);
                
                GetSet[][][] collection;
                if (!PropertyAccess.TryGetValue(type, out collection))
                {
                    collection = new GetSet[3][][];
                    PropertyAccess[type] = collection;
                }
                var accessors = collection[index];
                if (accessors == null)
                {
                    var vanilla = GetVanilla(type);

                    var acs = new List<GetSet>();
                    var props = SilverlightSerializer.GetPropertyInfo(type);
                    foreach (var p in props)
                    {
                        var gs = typeof (GetSetGeneric<,>);
                        var tp = gs.MakeGenericType(new Type[] { Type.GetTypeFromHandle(type), p.PropertyType });
                        var getSet = (GetSet) Activator.CreateInstance(tp, new object[] {p});
                        getSet.Vanilla = getSet.Get(vanilla);
                        acs.Add(getSet);
                    }
                    accessors = new GetSet[2][];
                    accessors[0] = acs.ToArray();
                    acs.Clear();
                    var fields = SilverlightSerializer.GetFieldInfo(type);
                    foreach (var f in fields)
                    {
                        var gs = typeof (GetSetGeneric<,>);
                        var tp = gs.MakeGenericType(new Type[] { Type.GetTypeFromHandle(type), f.FieldType });
                        var getSet = (GetSet) Activator.CreateInstance(tp, new object[] {f});
                        getSet.Vanilla = getSet.Get(vanilla);
                        acs.Add(getSet);
                    }
                    accessors[1] = acs.ToArray();

                    collection[index] = accessors;
                }
                return accessors;
            }
        }
    }
}