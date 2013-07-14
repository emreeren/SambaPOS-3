using System;
using System.Runtime.Serialization;

namespace Samba.Domain.Models.Entities
{
    [DataContract]
    public class CustomDataValue
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Value { get; set; }

        public EntityCustomField CustomField { get; set; }
    }

}