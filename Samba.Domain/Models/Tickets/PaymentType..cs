﻿using System.Collections.Generic;
using Samba.Domain.Models.Accounts;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Tickets
{
    public class PaymentType : EntityClass, IOrderable
    {
        public PaymentType()
        {
            _paymentTypeMaps = new List<PaymentTypeMap>();
        }

        public int SortOrder { get; set; }
        public string UserString { get { return Name; } }
        public string ButtonColor { get; set; }
        public virtual AccountTransactionType AccountTransactionType { get; set; }
        public virtual Account Account { get; set; }
        
        private readonly IList<PaymentTypeMap> _paymentTypeMaps;
        public virtual IList<PaymentTypeMap> PaymentTypeMaps
        {
            get { return _paymentTypeMaps; }
        }

        public PaymentTypeMap AddPaymentTypeMap()
        {
            var map = new PaymentTypeMap();
            PaymentTypeMaps.Add(map);
            return map;
        }
    }
}
