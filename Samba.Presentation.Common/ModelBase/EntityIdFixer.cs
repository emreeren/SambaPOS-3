using System;
using System.Collections;
using System.Linq;
using Samba.Infrastructure.Data;

namespace Samba.Presentation.Common.ModelBase
{
    public static class EntityIdFixer
    {
        public static void FixEntityIdNumber(IEntityClass obj, Func<Type, int> iDCreator)
        {
            obj.GetType()
               .GetProperties()
               .Where(x => x.PropertyType.IsGenericType && x.PropertyType.GetInterfaces().Contains(typeof(IEnumerable)))
               .ToList().ForEach(x => CollectionIdUpdater.UpdateCollectionIds(obj.GetType().Name, obj.Id, x, obj, iDCreator));
        }
    }
}