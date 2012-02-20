using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Tickets
{
    public class OrderTagTemplate : Entity
    {
        
        private IList<OrderTagTemplateValue> _orderTagTemplateValues;
        public virtual IList<OrderTagTemplateValue> OrderTagTemplateValues
        {
            get { return _orderTagTemplateValues; }
            set { _orderTagTemplateValues = value; }
        }

        public OrderTagTemplate()
        {
            _orderTagTemplateValues = new List<OrderTagTemplateValue>();
        }
    }
}
