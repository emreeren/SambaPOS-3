using System;
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
