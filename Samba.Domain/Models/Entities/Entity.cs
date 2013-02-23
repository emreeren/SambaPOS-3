using System;
using System.Text.RegularExpressions;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Entities
{
    public class Entity : EntityClass, ICacheable
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
    }
}
