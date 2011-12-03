using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Accounts
{
    public class AccountCustomField : IEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int FieldType { get; set; }
        public string DisplayFormat { get; set; }
        public string EditingFormat { get; set; }
    }
}
