using System.Linq;
using System.Runtime.Serialization;

namespace Samba.Domain.Models.Tickets
{
    [DataContract]
    public class TicketTagValue
    {
        [DataMember(Name = "TN")]
        public string TagName { get; set; }
        [DataMember(Name = "TV")]
        public string TagValue { get; set; }
        public string TagNameShort { get { return string.Join("", TagName.Where(char.IsUpper)); } }
    }
}
