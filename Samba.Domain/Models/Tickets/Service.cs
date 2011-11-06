using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Samba.Domain.Models.Tickets
{
    public class Service
    {
        public int Id { get; set; }
        public int TicketId { get; set; }
        public int ServiceId { get; set; }
        public int ServiceType { get; set; }
        public int CalculationType { get; set; }
        public decimal Amount { get; set; }
        public decimal CalculationAmount { get; set; }
    }
}
