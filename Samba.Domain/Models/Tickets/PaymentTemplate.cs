using System.Collections.Generic;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Settings;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Tickets
{
    public class PaymentTemplate : Entity, IOrderable
    {
        public PaymentTemplate()
        {
            _paymentTemplateMaps = new List<PaymentTemplateMap>();
        }

        public int Order { get; set; }
        public string UserString { get { return Name; } }
        public string ButtonColor { get; set; }
        public virtual AccountTransactionTemplate AccountTransactionTemplate { get; set; }
        public virtual Account Account { get; set; }
        
        private readonly IList<PaymentTemplateMap> _paymentTemplateMaps;
        public virtual IList<PaymentTemplateMap> PaymentTemplateMaps
        {
            get { return _paymentTemplateMaps; }
        }

        public PaymentTemplateMap AddPaymentTemplateMap()
        {
            var map = new PaymentTemplateMap();
            PaymentTemplateMaps.Add(map);
            return map;
        }
    }
}
