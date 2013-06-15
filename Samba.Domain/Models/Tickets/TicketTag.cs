using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Infrastructure;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Tickets
{
    public class TicketTag : EntityClass, IStringCompareable
    {
        private static TicketTag _emptyTicketTag;

        public static TicketTag Empty
        {
            get
            {
                return _emptyTicketTag ?? (_emptyTicketTag = new TicketTag());
            }
        }

        public int TicketTagGroupId { get; set; }

        public string Display
        {
            get
            {
                return !string.IsNullOrEmpty(Name) ? Name : "X";
            }
        }

        public string GetStringValue()
        {
            return Name;
        }
    }
}
