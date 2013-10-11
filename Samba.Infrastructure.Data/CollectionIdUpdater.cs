using System;
using System.Collections;
using System.Linq;
using System.Reflection;

namespace Samba.Infrastructure.Data
{
    public static class CollectionIdUpdater
    {
        public static void UpdateCollectionIds(string className, int id, PropertyInfo propertyInfo, object o,
                                            Func<Type, int> iDCreator)
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
                        .ToList().ForEach(x => UpdateCollectionIds(type.Name, vitem.Id, x, item, iDCreator));
                }

            }
        }
    }
}