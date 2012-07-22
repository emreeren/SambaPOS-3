using System;
using System.Text.RegularExpressions;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Resources
{
    public class Resource : Entity, ICacheable
    {
        public int ResourceTemplateId { get; set; }
        public DateTime LastUpdateTime { get; set; }
        public string SearchString { get; set; }
        public string CustomData { get; set; }
        public int AccountId { get; set; }

        public string GetCustomData(string fieldName)
        {
            if (string.IsNullOrEmpty(CustomData)) return "";
            var pattern = string.Format("\"Name\":\"{0}\",\"Value\":\"([^\"]+)\"", fieldName);
            return Regex.IsMatch(CustomData, pattern)
                ? Regex.Match(CustomData, pattern).Groups[1].Value : "";
        }

        private static Resource _null;
        public static Resource Null { get { return _null ?? (_null = new Resource { Name = "*" }); } }

        public static Resource GetNullResource(int resourceTemplateId)
        {
            var result = Null;
            result.ResourceTemplateId = resourceTemplateId;
            return Null;
        }

        public Resource()
        {
            LastUpdateTime = DateTime.Now;
        }
    }
}
