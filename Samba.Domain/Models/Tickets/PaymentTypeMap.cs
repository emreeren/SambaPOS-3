using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Tickets
{
    public class PaymentTypeMap : AbstractMap
    {
        public int PaymentTypeId { get; set; }
        public bool DisplayAtPaymentScreen { get; set; }
        public bool DisplayUnderTicket { get; set; }
    }
}
