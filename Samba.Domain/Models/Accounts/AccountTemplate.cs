using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Accounts
{
    public class AccountTemplate : Entity,IOrderable
    {
        public int DefaultFilterType { get; set; }
        public int Order { get; set; }
        public string UserString { get { return Name; } }
        public string Tags { get; set; }
    }
}
