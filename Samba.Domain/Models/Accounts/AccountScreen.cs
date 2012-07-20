using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Accounts
{
    public class AccountScreen : Entity
    {
        public string AccountTemplateNames { get; set; }
        public IEnumerable<string> AccountTemplateNamesList { get { return (AccountTemplateNames??"").Split(';'); } }
    }
}
