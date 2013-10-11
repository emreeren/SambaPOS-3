using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Samba.Infrastructure.Data.Text
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
                       .ToList().ForEach(x => CollectionIdUpdater.UpdateCollectionIds(className, id, x, obj, iDCreator));
                }
            }
        }
    }
}