using System;
using System.Runtime.Serialization;

namespace Samba.Domain.Models.Tickets
{
    [DataContract]
    public class OrderStateValue : IEquatable<OrderStateValue>
    {
        [DataMember(Name = "SN")]
        public string StateName { get; set; }
        [DataMember(Name = "S")]
        public string State { get; set; }
        [DataMember(Name = "SV")]
        public string StateValue { get; set; }
        [DataMember(Name = "OK", EmitDefaultValue = true)]
        public string OrderKey { get; set; }

        private static OrderStateValue _default;
        public static OrderStateValue Default
        {
            get { return _default ?? (_default = new OrderStateValue()); }
        }

        public bool Equals(OrderStateValue other)
        {
            if (other == null) return false;
            return other.State == State && other.StateName == StateName;
        }

        public override int GetHashCode()
        {
            return (StateName + "_" + StateValue).GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var item = obj as OrderStateValue;
            return item != null && Equals(item);
        }
    }
}