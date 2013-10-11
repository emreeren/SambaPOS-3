using System;
using System.Collections.Generic;
using System.Xml;
using Omu.ValueInjecter;
using Samba.Infrastructure.Data.Injection;

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
                    var dictionary = new Dictionary<Type, Dictionary<int, IEntityClass>>();
                    PropertyComparor.ExtractEntities(item, dictionary);

                    var xmlDocument = serializer.Serialize(item);
                    var clone = deserializer.Deserialize(xmlDocument, dictionary) as T;
                    return clone;
                }
            }
        }

        public static T Clone2<T>(T item) where T : new()
        {
            var result = new T();
            result.InjectFrom<CloneInjection>(item);
            return result;
        }

        public static T EntityClone<T>(T item) where T : new()
        {
            var result = new T();
            result.InjectFrom<EntityInjection>(item);
            return result;
        }

        public static T Deserialize<T>(string data) where T : class
        {
            using (var deserializer = new XmlDeserializerHelper())
            {
                var d = new XmlDocument();
                d.LoadXml(data);
                return deserializer.Deserialize(d) as T;
            }
        }

        public static string Serialize<T>(T data) where T : class
        {
            using (var serializer = new XmlSerializerHelper())
            {
                var doc = serializer.Serialize(data);
                return doc.InnerXml;
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
