using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Samba.Modules.BasicReports.Reports
{
    internal class AmountCalculator
    {
        private readonly IEnumerable<TenderedAmount> _amounts;
        public AmountCalculator(IEnumerable<TenderedAmount> amounts)
        {
            _amounts = amounts;
        }

        internal decimal GetAmount(int paymentType)
        {
            var r = _amounts.SingleOrDefault(x => x.PaymentType == paymentType);
            return r != null ? r.Amount : 0;
        }

        internal string GetPercent(int paymentType)
        {
            return TotalAmount > 0 ? string.Format("%{0:0.00}", (GetAmount(paymentType) * 100) / TotalAmount) : "%0";
        }

        public decimal CashTotal { get { return GetAmount(0); } }
        public decimal CreditCardTotal { get { return GetAmount(1); } }
        public decimal TicketTotal { get { return GetAmount(2); } }
        public decimal AccountTotal { get { return GetAmount(3); } }
        public decimal GrandTotal { get { return _amounts.Where(x => x.PaymentType != 3).Sum(x => x.Amount); } }
        public decimal TotalAmount { get { return _amounts.Sum(x => x.Amount); } }

        public string CashPercent { get { return GetPercent(0); } }
        public string CreditCardPercent { get { return GetPercent(1); } }
        public string TicketPercent { get { return GetPercent(2); } }
        public string AccountPercent { get { return GetPercent(3); } }
    }
}
