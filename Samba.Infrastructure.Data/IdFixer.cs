using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Samba.Infrastructure.Data
{
    public static class IdFixer
    {
        public static void FixIdNumbers(Dictionary<string, Dictionary<int, object>> items, Func<Type, int> iDCreator)
        {
            foreach (var item in items)
            {
                var parts = item.Key.Split('.');
                var className = parts[parts.Length - 1];
                foreach (var listItem in item.Value)
                {
                    var id = listItem.Key;
                    var obj = listItem.Value;
                    obj.GetType()
                       .GetProperties()
                       .Where(x => x.PropertyType.IsGenericType && x.PropertyType.GetInterfaces().Contains(typeof(IEnumerable)))
                       .ToList().ForEach(x => UpdateCollection(className, id, x, obj, iDCreator));
                }
            }
        }

        public static void FixEntityIdNumber(IEntityClass obj, Func<Type, int> iDCreator)
        {
            obj.GetType()
               .GetProperties()
               .Where(x => x.PropertyType.IsGenericType && x.PropertyType.GetInterfaces().Contains(typeof(IEnumerable)))
               .ToList().ForEach(x => UpdateCollection(obj.GetType().Name, obj.Id, x, obj, iDCreator));
        }

        private static void UpdateCollection(string className, int id, PropertyInfo propertyInfo, object o, Func<Type, int> iDCreator)
        {
            var collection = propertyInfo.GetValue(o, null) as IEnumerable;
            foreach (var item in collection)
            {
                var prop = item.GetType().GetProperties().FirstOrDefault(x => x.Name == className + "Id");
                if (prop != null)
                {
                    prop.SetValue(item, id, null);
                }
                var vitem = item as IValueClass;
                if (vitem != null)
                {
                    var type = vitem.GetType();
                    if (vitem.Id == 0) vitem.Id = iDCreator(type);
                    type.GetProperties()
                        .Where(
                            x =>
                            x.PropertyType.IsGenericType &&
                            x.PropertyType.GetInterfaces().Contains(typeof(IEnumerable)))
                        .ToList().ForEach(x => UpdateCollection(type.Name, vitem.Id, x, item, iDCreator));
                }

            }
        }
    }
}