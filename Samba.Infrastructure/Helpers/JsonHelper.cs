using System;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.RegularExpressions;

namespace Samba.Infrastructure.Helpers
{
    public class JsonHelper
    {
        public static string Serialize<T>(T obj)
        {
            using (var stream = new MemoryStream())
            {
                var serializer = new DataContractJsonSerializer(obj.GetType());
                serializer.WriteObject(stream, obj);
                return Encoding.UTF8.GetString(stream.ToArray());
            }
        }

        public static T Deserialize<T>(string json) where T : new()
        {
            if (string.IsNullOrEmpty(json)) return new T();
            json = EscapeStringValue(json);
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(json)))
            {
                var serializer = new DataContractJsonSerializer(typeof(T));
                return (T)serializer.ReadObject(stream);
            }
        }

        public static string EscapeStringValue(string value)
        {
            var result = value;
            result = result.Replace("\r", "\\r");
            result = result.Replace("\n", "\\n");
            result = result.Replace("\t", "\\t");
            return result;
        }
    }
}
