using System;
using System.Runtime.Serialization;

namespace Samba.Domain.Models.Tickets
{
    [DataContract]
    public class OrderStateValue : IEquatable<OrderStateValue>
    {
        public OrderStateValue()
        {
            LastUpdateTime = DateTime.Now;
        }

        [DataMember(Name = "SN")]
        public string StateName { get; set; }
        [DataMember(Name = "S")]
        public string State { get; set; }
        [DataMember(Name = "SV")]
        public string StateValue { get; set; }
        [DataMember(Name = "OK", EmitDefaultValue = false)]
        public string OrderKey { get; set; }
        [DataMember(Name = "D", IsRequired = false, EmitDefaultValue = false)]
        public DateTime LastUpdateTime { get; set; }
        [DataMember(Name = "U", IsRequired = false, EmitDefaultValue = false)]
        public int UserId { get; set; }

        private static OrderStateValue _default;
        public static OrderStateValue Default
        {
            get { return _default ?? (_default = new OrderStateValue() { OrderKey = "", StateName = "", StateValue = "", State = "" }); }
        }

        public bool Equals(OrderStateValue other)
        {
            if (other == null) return false;
            return other.State == State && other.StateName == StateName && other.StateValue == StateValue;
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