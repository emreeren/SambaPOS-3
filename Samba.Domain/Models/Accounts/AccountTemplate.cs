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

        private readonly IList<AccountCustomField> _accountCustomFields;
        public virtual IList<AccountCustomField> AccountCustomFields
        {
            get { return _accountCustomFields; }
        }

        public int DefaultFilterType { get; set; }

        public AccountTemplate()
        {
            _accountCustomFields = new List<AccountCustomField>();
        }

        public AccountCustomField AddCustomField(string fieldName, int fieldType)
        {
            var result = new AccountCustomField { Name = fieldName, FieldType = fieldType };
            _accountCustomFields.Add(result);
            return result;
        }
    }
}
