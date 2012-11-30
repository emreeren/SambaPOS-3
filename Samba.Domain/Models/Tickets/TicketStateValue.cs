using System.Runtime.Serialization;

namespace Samba.Domain.Models.Tickets
{
    [DataContract]
    public class TicketStateValue
    {
        [DataMember(Name = "SN")]
        public string StateName { get; set; }
        [DataMember(Name = "S")]
        public string State { get; set; }
        [DataMember(Name = "SV", EmitDefaultValue = false)]
        public string StateValue { get; set; }
        [DataMember(Name = "Q", EmitDefaultValue = false)]
        public int Quantity { get; set; }

        private static TicketStateValue _default;
        public static TicketStateValue Default
        {
            get { return _default ?? (_default = new TicketStateValue()); }
        }
    }
}