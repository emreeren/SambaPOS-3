using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Tickets
{
    public class PaymentTemplateMap : Value, IAbstractMapModel
    {
        public int PaymentTemplateId { get; set; }
        public int UserRoleId { get; set; }
        public int DepartmentId { get; set; }
        public bool DisplayAtPaymentScreen { get; set; }
        public bool DisplayUnderTicket { get; set; }
    }
}
