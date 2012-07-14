using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Tickets
{
    public class TicketResource : Value
    {
        public int ResourceTemplateId { get; set; }
        public int ResourceId { get; set; }
        public int AccountId { get; set; }
        public string ResourceName { get; set; }
    }
}
