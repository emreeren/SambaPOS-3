using System;
using System.Collections.Generic;
using System.IO;
using Samba.Infrastructure.Helpers;

namespace Samba.Presentation.Common.ModelBase
{
    public static class EntityCollectionSortManager
    {
        private static SerializableDictionary<string, bool> _items = new SerializableDictionary<string, bool>();
        private static string _fileName;

        public static bool GetOrderByDesc<T>()
        {
            var typeName = (typeof(T)).Name;
            if (!_items.ContainsKey(typeName))
            {
                return false;
            }
            return _items[typeName];
        }

        public static void SetOrderByDesc<T>(bool value)
        {
            var typeName = (typeof(T)).Name;
            if (!_items.ContainsKey(typeName))
            {
                _items.Add(typeName, value);
            }
            else
            {
                _items[typeName] = value;
            }
            Save();
        }

        public static void Save()
        {
            if (string.IsNullOrEmpty(_fileName)) return;
            var text = JsonHelper.Serialize(_items);
            File.WriteAllText(_fileName, text);
        }

        public static void Load(string fileName)
        {
            _fileName = fileName;
            try
            {
                var text = File.ReadAllText(_fileName);
                _items = JsonHelper.Deserialize<SerializableDictionary<string, bool>>(text);
            }
            catch (Exception)
            {
                _items = new SerializableDictionary<string, bool>();
            }
        }
    }
}