using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Samba.Infrastructure.Data;
using Samba.Infrastructure.Helpers;

namespace Samba.Domain.Models.Resources
{
    public class Resource : Entity, ICacheable
    {
        public int ResourceTypeId { get; set; }
        public DateTime LastUpdateTime { get; set; }
        public string SearchString { get; set; }
        public string CustomData { get; set; }
        public int AccountId { get; set; }

        public string ResourceStates { get; set; }

        private IList<ResourceStateVal> _resourceStateValues;
        internal IList<ResourceStateVal> ResourceStateValues
        {
            get { return _resourceStateValues ?? (_resourceStateValues = JsonHelper.Deserialize<List<ResourceStateVal>>(ResourceStates)); }
        }

        public ResourceStateVal GetStateValue(string stateName)
        {
            return ResourceStateValues.SingleOrDefault(x => x.StateName == stateName) ?? ResourceStateVal.Default;
        }

        public void SetStateValue(string groupName, string state)
        {
            var sv = ResourceStateValues.SingleOrDefault(x => x.StateName == groupName);
            if (sv == null)
            {
                sv = new ResourceStateVal { StateName = groupName, State = state };
                ResourceStateValues.Add(sv);
            }
            else
            {
                sv.State = state;
            }

            if (string.IsNullOrEmpty(sv.State))
                ResourceStateValues.Remove(sv);

            ResourceStates = JsonHelper.Serialize(ResourceStateValues);
            _resourceStateValues = null;
        }

        public string GetCustomData(string fieldName)
        {
            return GetCustomData(CustomData, fieldName);
        }

        private static Resource _null;
        public static Resource Null { get { return _null ?? (_null = new Resource { Name = "*" }); } }

        public static Resource GetNullResource(int resourceTypeId)
        {
            var result = Null;
            result.ResourceTypeId = resourceTypeId;
            return Null;
        }

        public Resource()
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

        public string GetStateData()
        {
            return string.Join("\r", ResourceStateValues.Where(x => !string.IsNullOrEmpty(x.State)).Select(x => string.Format("{0}:{1}", x.StateName, x.State)));
        }
    }
}
