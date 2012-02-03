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

        internal decimal GetAmount(string paymentName)
        {
            var r = _amounts.SingleOrDefault(x => x.PaymentName == paymentName);
            return r != null ? r.Amount : 0;
        }

        internal string GetPercent(string paymentName)
        {
            return TotalAmount > 0 ? string.Format("%{0:0.00}", (GetAmount(paymentName) * 100) / TotalAmount) : "%0";
        }

        public IEnumerable<string> PaymentNames { get { return _amounts.Select(x => x.PaymentName).Distinct(); } }
        public decimal TotalAmount { get { return _amounts.Sum(x => x.Amount); } }
    }
}
