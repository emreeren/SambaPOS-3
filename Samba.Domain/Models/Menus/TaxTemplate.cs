using System.Collections.Generic;
using Samba.Domain.Models.Accounts;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Menus
{
    public class TaxTemplate : EntityClass, IOrderable
    {
        public TaxTemplate()
        {
            _taxTemplateMaps = new List<TaxTemplateMap>();
        }

        public int SortOrder { get; set; }
        public string UserString { get { return Name; } }
        public decimal Rate { get; set; }
        public int Rounding { get; set; }
        public virtual AccountTransactionType AccountTransactionType { get; set; }

        private IList<TaxTemplateMap> _taxTemplateMaps;
        public virtual IList<TaxTemplateMap> TaxTemplateMaps
        {
            get { return _taxTemplateMaps; }
            set { _taxTemplateMaps = value; }
        }
    }
}
