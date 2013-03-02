﻿using System.Collections.Generic;
using System.Linq;
using Samba.Infrastructure.Data;
using Samba.Infrastructure.Helpers;

namespace Samba.Domain.Models.Entities
{
    public class EntityStateValue : ValueClass
    {
        public int EntityId { get; set; }
        public string EntityStates { get; set; }

        private IList<EntityStateVal> _entityStateValues;
        internal IList<EntityStateVal> EntityStateValues
        {
            get { return _entityStateValues ?? (_entityStateValues = JsonHelper.Deserialize<List<EntityStateVal>>(EntityStates)); }
        }

        public string GetStateValue(string stateName)
        {
            var sv= EntityStateValues.SingleOrDefault(x => x.StateName == stateName) ?? EntityStateVal.Default;
            return sv.State;
        }

        public void SetStateValue(string groupName, string state)
        {
            var sv = EntityStateValues.SingleOrDefault(x => x.StateName == groupName);
            if (sv == null)
            {
                sv = new EntityStateVal { StateName = groupName, State = state };
                EntityStateValues.Add(sv);
            }
            else
            {
                sv.State = state;
            }

            if (string.IsNullOrEmpty(sv.State))
                EntityStateValues.Remove(sv);

            EntityStates = JsonHelper.Serialize(EntityStateValues);
            _entityStateValues = null;
        }

        public string GetStateData()
        {
            return string.Join("\r", EntityStateValues.Where(x => !string.IsNullOrEmpty(x.State)).Select(x => string.Format("{0}:{1}", x.StateName, x.State)));
        }

        public bool IsInState(string stateName, string state)
        {
            if (string.IsNullOrEmpty(stateName)) return true;
            if (stateName == "*") return EntityStateValues.Any(x => x.State == state);
            if (string.IsNullOrEmpty(state)) return EntityStateValues.All(x => x.StateName != stateName);
            return EntityStateValues.Any(x => x.StateName == stateName && x.State == state);

        }
    }
}
