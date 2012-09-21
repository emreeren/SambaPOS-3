using System.Collections.Generic;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Settings;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Tickets
{
    public class ChangePaymentTemplate : Entity, IOrderable
    {
        public ChangePaymentTemplate()
        {
            _changePaymentTemplateMaps = new List<ChangePaymentTemplateMap>();
        }

        public int Order { get; set; }
        public string UserString { get { return Name; } }
        public virtual AccountTransactionTemplate AccountTransactionTemplate { get; set; }
        public virtual Account Account { get; set; }
        
        private readonly IList<ChangePaymentTemplateMap> _changePaymentTemplateMaps;
        public virtual IList<ChangePaymentTemplateMap> ChangePaymentTemplateMaps
        {
            get { return _changePaymentTemplateMaps; }
        }

        public ChangePaymentTemplateMap AddPChangeaymentTemplateMap()
        {
            var map = new ChangePaymentTemplateMap();
            ChangePaymentTemplateMaps.Add(map);
            return map;
        }

    }
}
