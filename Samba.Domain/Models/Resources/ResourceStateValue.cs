using System;
using System.Collections.Generic;
using System.Linq;
using Samba.Infrastructure.Data;
using Samba.Infrastructure.Helpers;

namespace Samba.Domain.Models.Resources
{
    public class ResourceStateValue : Value
    {
        public int ResoruceId { get; set; }
        public DateTime Date { get; set; }
        public string ResourceStates { get; set; }

        private IList<ResourceStateVal> _resourceStateValues;
        internal IList<ResourceStateVal> ResourceStateValues
        {
            get { return _resourceStateValues ?? (_resourceStateValues = JsonHelper.Deserialize<List<ResourceStateVal>>(ResourceStates)); }
        }

        public string GetStateValue(string stateName)
        {
            var sv= ResourceStateValues.SingleOrDefault(x => x.StateName == stateName) ?? ResourceStateVal.Default;
            return sv.State;
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

        public string GetStateData()
        {
            return string.Join("\r", ResourceStateValues.Where(x => !string.IsNullOrEmpty(x.State)).Select(x => string.Format("{0}:{1}", x.StateName, x.State)));
        }
    }
}
