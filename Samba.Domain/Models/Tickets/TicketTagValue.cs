using System;
using System.Linq;
using System.Runtime.Serialization;

namespace Samba.Domain.Models.Tickets
{
    [DataContract]
    public class TicketTagValue : IEquatable<TicketTagValue>
    {
        [DataMember(Name = "TN")]
        public string TagName { get; set; }
        [DataMember(Name = "TV")]
        public string TagValue { get; set; }

        public string TagNameShort { get { return string.Join("", TagName.Where(char.IsUpper)); } }

        public bool Equals(TicketTagValue other)
        {
            if (other == null) return false;
            return other.TagName == TagName && other.TagValue == TagValue;
        }

        public override int GetHashCode()
        {
            return (TagName + "_" + TagValue).GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var item = obj as TicketTagValue;
            return item != null && Equals(item);
        }
    }
}
