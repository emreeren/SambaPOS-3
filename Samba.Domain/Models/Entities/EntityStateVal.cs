using System.Runtime.Serialization;

namespace Samba.Domain.Models.Entities
{
    [DataContract]
    public class EntityStateVal
    {
        [DataMember(Name = "SN")]
        public string StateName { get; set; }
        [DataMember(Name = "S")]
        public string State { get; set; }

        private static EntityStateVal _default;
        public static EntityStateVal Default
        {
            get
            {
                return _default ?? (_default = new EntityStateVal());
            }
        }
    }
}