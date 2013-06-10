using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Samba.ApiServer.Lib
{
    public static class SimpleSerializer
    {
        public static void SerializeToXmlFile<T>(string fileName, T dataObject)
        {
            if (!File.Exists(fileName))
            {
                File.Create(fileName).Close();
            }

            XmlSerializer serializer = new XmlSerializer(typeof(T));
            XmlTextWriter writer = new XmlTextWriter(fileName, null);
            try
            {
                serializer.Serialize(writer, dataObject);
            }
            finally
            {
                writer.Close();
            }
        }

        public static T DeserializeFromXmlFile<T>(string fileName)
        {
            T returnT;

            if (File.Exists(fileName))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(Token));
                XmlTextReader reader = new XmlTextReader(fileName);
                try
                {
                    returnT = (T)serializer.Deserialize(reader);
                }
                finally
                {
                    reader.Close();
                }
            }
            else
            {
                throw new ArgumentOutOfRangeException();
            }
            return returnT;
        }
    }
}
