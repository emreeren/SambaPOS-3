using System.Collections.Generic;
using Samba.Domain.Models.Accounts;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Tickets
{
    public class CalculationTemplate : Entity, IOrderable
    {
        public int Order { get; set; }

        public string UserString
        {
            get { return Name; }
        }

        public int CalculationMethod { get; set; }
        public decimal Amount { get; set; }
        public decimal MaxAmount { get; set; }
        public bool IncludeTax { get; set; }
        public bool DecreaseAmount { get; set; }
        public virtual AccountTransactionTemplate AccountTransactionTemplate { get; set; }
    }
}
