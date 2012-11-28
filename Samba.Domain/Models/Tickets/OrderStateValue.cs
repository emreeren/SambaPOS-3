using System.Runtime.Serialization;

namespace Samba.Domain.Models.Tickets
{
    [DataContract]
    public class OrderStateValue
    {
        [DataMember(Name = "GN")]
        public string GroupName { get; set; }
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
    }
}