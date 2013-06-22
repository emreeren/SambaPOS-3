using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Menus
{
    public class ProductTimer : EntityClass
    {
        public int PriceType { get; set; }
        public decimal PriceDuration { get; set; }
        public decimal MinTime { get; set; }
        public decimal TimeRounding { get; set; }
        public int StartTime { get; set; }

        private IList<ProdcutTimerMap> _productTimerMaps;
        public virtual IList<ProdcutTimerMap> ProductTimerMaps
        {
            get { return _productTimerMaps ?? (_productTimerMaps = new List<ProdcutTimerMap>()); }
            set { _productTimerMaps = value; }
        }

    }
}
