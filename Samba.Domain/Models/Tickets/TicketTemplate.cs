using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Domain.Models.Settings;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Tickets
{
    public class TicketTemplate : IEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public virtual Numerator TicketNumerator { get; set; }
        public virtual Numerator OrderNumerator { get; set; }
    }
}
