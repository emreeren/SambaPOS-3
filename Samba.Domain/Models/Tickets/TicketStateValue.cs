using System;
using System.Runtime.Serialization;

namespace Samba.Domain.Models.Tickets
{
    [DataContract]
    public class TicketStateValue : IEquatable<TicketStateValue>
    {
        public TicketStateValue()
        {
            LastUpdateTime = DateTime.Now;
        }

        [DataMember(Name = "SN")]
        public string StateName { get; set; }
        [DataMember(Name = "S")]
        public string State { get; set; }
        [DataMember(Name = "SV", EmitDefaultValue = false)]
        public string StateValue { get; set; }
        [DataMember(Name = "Q", EmitDefaultValue = false)]
        public int Quantity { get; set; }
        [DataMember(Name = "D", IsRequired = false, EmitDefaultValue = false)]
        public DateTime LastUpdateTime { get; set; }

        private static TicketStateValue _default;
        public static TicketStateValue Default
        {
            get { return _default ?? (_default = new TicketStateValue()); }
        }

        public bool Equals(TicketStateValue other)
        {
            if (other == null) return false;
            return other.StateName == StateName && other.State == State;
        }

        public override int GetHashCode()
        {
            return (StateName + "_" + StateValue).GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var item = obj as TicketStateValue;
            return item != null && Equals(item);
        }
    }
}