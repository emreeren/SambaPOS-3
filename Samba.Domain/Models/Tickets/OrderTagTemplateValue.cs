using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Tickets
{
    public class OrderTagTemplateValue : IEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public virtual OrderTagGroup OrderTagGroup { get; set; }
        public virtual OrderTag OrderTag { get; set; }
    }
}
