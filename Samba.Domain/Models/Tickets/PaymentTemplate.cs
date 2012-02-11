using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Domain.Models.Accounts;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Tickets
{
    public class PaymentTemplate : IEntity, IOrderable
    {
        public PaymentTemplate()
        {
            ButtonColor = "Gainsboro";
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public int Order { get; set; }
        public string UserString { get { return Name; } }
        public string ButtonColor { get; set; }
        public virtual AccountTransactionTemplate AccountTransactionTemplate { get; set; }
        public virtual Account Account { get; set; }
    }
}
