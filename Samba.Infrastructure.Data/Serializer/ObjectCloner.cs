using System;
using System.Collections.Generic;

namespace Samba.Infrastructure.Data.Serializer
{
    public static class ObjectCloner
    {
        public static T Clone<T>(T item) where T : class
        {
            using (var serializer = new XmlSerializerHelper { IgnoreSerializableAttribute = true })
            {
                using (var deserializer = new XmlDeserializerHelper())
                {
                    var dictionary = new Dictionary<Type, Dictionary<int, IEntity>>();
                    PropertyComparor.ExtractEntities(item, dictionary);

                    var xmlDocument = serializer.Serialize(item);
                    var clone = deserializer.Deserialize(xmlDocument, dictionary) as T;
                    return clone;
                }
            }
        }

        public static int DataHash(object item)
        {
            using (var serializer = new XmlSerializerHelper { IgnoreSerializableAttribute = true })
            {
                var xmlDoc = serializer.Serialize(item);
                return xmlDoc.InnerXml.GetHashCode();
            }
        }
    }
}
