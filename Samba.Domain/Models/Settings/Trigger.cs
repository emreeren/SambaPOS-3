using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Settings
{
    public class Trigger : IEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Expression { get; set; }
        public DateTime LastTrigger { get; set; }
    }
}
