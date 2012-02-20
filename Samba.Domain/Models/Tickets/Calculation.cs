using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Tickets
{
    public class Calculation : Value
    {
        public string Name { get; set; }
        public int Order { get; set; }
        public int ServiceId { get; set; }
        public int TicketId { get; set; }
        public int AccountTransactionTemplateId { get; set; }
        public int CalculationType { get; set; }
        public bool IncludeTax { get; set; }
        public bool DecreaseAmount { get; set; }
        public decimal Amount { get; set; }
        public decimal CalculationAmount { get; set; }
    }
}
