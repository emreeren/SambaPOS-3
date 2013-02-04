using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Tickets
{
    public class OrderTagTemplateValue : ValueClass
    {
        public int OrderTagTemplateId { get; set; }
        public virtual OrderTagGroup OrderTagGroup { get; set; }
        public virtual OrderTag OrderTag { get; set; }
    }
}
