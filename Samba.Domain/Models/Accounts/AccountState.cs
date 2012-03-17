using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Accounts
{
    public class AccountState : Entity
    {
        public int AccountTemplateId { get; set; }
        public string Color { get; set; }
    }
}
