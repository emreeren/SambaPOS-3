using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Samba.Infrastructure.Data;
using Samba.Infrastructure.Helpers;

namespace Samba.Domain.Models.Entities
{
    public class Entity : EntityClass, ICacheable, ICustomDataProvider
    {
        public int EntityTypeId { get; set; }
        public DateTime LastUpdateTime { get; set; }
        public string SearchString { get; set; }
        public string CustomData { get; set; }
        public int AccountId { get; set; }
        public int WarehouseId { get; set; }

        public string GetCustomData(string fieldName)
        {
            return GetCustomData(CustomData, fieldName);
        }

        private static Entity _null;
        public static Entity Null { get { return _null ?? (_null = new Entity { Name = "*" }); } }

        public static Entity GetNullEntity(int entityTypeId)
        {
            var result = Null;
            result.EntityTypeId = entityTypeId;
            return Null;
        }

        public Entity()
        {
            LastUpdateTime = DateTime.Now;
        }

        public static string GetCustomData(string customData, string fieldName)
        {
            if (string.IsNullOrEmpty(customData)) return "";
            var pattern = string.Format("\"Name\":\"{0}\",\"Value\":\"([^\"]+)\"", fieldName);
            return Regex.IsMatch(customData, pattern)
                ? Regex.Match(customData, pattern).Groups[1].Value : "";
        }

        public void SetCustomData(string fieldName, string value)
        {
            value = QuantityFuncParser.Parse(value, GetCustomData(fieldName));
            value = DateFuncParser.Parse(value, GetCustomData(fieldName));
            var list = JsonHelper.Deserialize<List<CustomDataValue>>(CustomData);
            if (list.All(x => x.Name != fieldName))
                list.Add(new CustomDataValue { Name = fieldName });
            list.Single(x => x.Name == fieldName).Value = value;
            CustomData = JsonHelper.Serialize(list);
        }

        public void SetDefaultValues(string data)
        {
            if (!string.IsNullOrEmpty(data) && data.Contains(":"))
            {
                var parts = data.Split(new[] { ':' }, 2);
                SetCustomData(parts[0], parts[1]);
            }
            else
            {
                Name = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(data ?? "");
            }
        }
    }
}
