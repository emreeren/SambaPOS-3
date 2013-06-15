﻿using System.Collections.Generic;
using Samba.Domain.Models.Accounts;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Tickets
{
    public class ChangePaymentType : EntityClass, IOrderable
    {
        private IList<ChangePaymentTypeMap> _changePaymentTypeMaps;

        public ChangePaymentType()
        {
            _changePaymentTypeMaps = new List<ChangePaymentTypeMap>();
        }

        public int SortOrder { get; set; }

        public string UserString
        {
            get
            {
                return Name;
            }
        }

        public virtual AccountTransactionType AccountTransactionType { get; set; }

        public virtual Account Account { get; set; }
        
        public virtual IList<ChangePaymentTypeMap> ChangePaymentTypeMaps
        {
            get
            {
                return _changePaymentTypeMaps;
            }
            set
            {
                _changePaymentTypeMaps = value;
            }
        }

        public ChangePaymentTypeMap AddPChangeaymentTemplateMap()
        {
            var map = new ChangePaymentTypeMap();
            ChangePaymentTypeMaps.Add(map);
            return map;
        }
    }
}
