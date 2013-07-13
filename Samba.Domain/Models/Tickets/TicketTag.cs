using Samba.Infrastructure;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Tickets
{
    public class TicketTag : EntityClass, IStringCompareable, IOrderable
    {
        public int TicketTagGroupId { get; set; }
        public int SortOrder { get; set; }

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

        public string UserString { get { return Name; } }
    }
}
