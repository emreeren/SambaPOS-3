using System.Runtime.Serialization;

namespace Samba.Domain.Models.Resources
{
    [DataContract]
    public class ResourceStateVal
    {
        [DataMember(Name = "SN")]
        public string StateName { get; set; }
        [DataMember(Name = "S")]
        public string State { get; set; }

        private static ResourceStateVal _default;
        public static ResourceStateVal Default
        {
            get
            {
                return _default ?? (_default = new ResourceStateVal());
            }
        }
    }
}