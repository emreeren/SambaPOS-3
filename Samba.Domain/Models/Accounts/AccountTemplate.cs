using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Accounts
{
    public class AccountTemplate : IEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
