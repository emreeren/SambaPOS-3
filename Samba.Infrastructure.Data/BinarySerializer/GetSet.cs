using System.Reflection;

namespace Samba.Infrastructure.Data.BinarySerializer
{
    public abstract class GetSet
    {
        public PropertyInfo Info;
        public string Name;
        public FieldInfo FieldInfo;
        public object Vanilla;
        public bool CollectionType;
        public abstract object Get(object item);
        public abstract void Set(object item, object value);

    }
}