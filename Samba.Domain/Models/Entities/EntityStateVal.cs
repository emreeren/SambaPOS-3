using System;
using System.Runtime.Serialization;

namespace Samba.Domain.Models.Entities
{
    [DataContract]
    public class EntityStateVal
    {
        public EntityStateVal()
        {
            LastUpdateTime = DateTime.Now;
        }

        [DataMember(Name = "SN")]
        public string StateName { get; set; }
        [DataMember(Name = "S")]
        public string State { get; set; }
        [DataMember(Name = "D", IsRequired = false, EmitDefaultValue = false)]
        public DateTime LastUpdateTime { get; set; }
        [DataMember(Name = "Q",EmitDefaultValue = false)]
        public int Quantity { get; set; }

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