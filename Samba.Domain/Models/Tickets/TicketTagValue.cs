using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Samba.Domain.Models.Tickets
{
    public class TicketTagValue
    {
        public int Id { get; set; }
        public int TicketId { get; set; }
        public string TagName { get; set; }
        public string TagValue { get; set; }
        public DateTime DateTime { get; set; }
    }
}
