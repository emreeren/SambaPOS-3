using Samba.Infrastructure;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Tickets
{
    public class TicketTag : EntityClass, IStringCompareable
    {
        public int TicketTagGroupId { get; set; }
        public string Display { get { return !string.IsNullOrEmpty(Name) ? Name : "X"; } }

        private static TicketTag _emptyTicketTag;
        public static TicketTag Empty
        {
            get { return _emptyTicketTag ?? (_emptyTicketTag = new TicketTag()); }
        }

        public string GetStringValue()
        {
            return Name;
        }
    }
}
