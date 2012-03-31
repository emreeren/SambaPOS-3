using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Accounts
{
    public class AccountTemplate : Entity
    {
        public int DefaultFilterType { get; set; }
    }
}
