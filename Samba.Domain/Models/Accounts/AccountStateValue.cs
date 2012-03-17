using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Accounts
{
    public class AccountStateValue : Value
    {
        public int AccountId { get; set; }
        public DateTime Date { get; set; }
        public int StateId { get; set; }
    }
}
