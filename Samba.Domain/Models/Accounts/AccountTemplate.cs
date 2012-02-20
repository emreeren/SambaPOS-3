using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Accounts
{
    public class AccountTemplate : Entity
    {
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

        public IEnumerable<AccountCustomField> GetMatchingFields(Account account, string searchString)
        {
            if (!string.IsNullOrEmpty(account.CustomData))
            {
                const string nameFormat = "\"Name\":\"{0}\"";
                var data = account.CustomData.Split(new[] { "}," }, StringSplitOptions.RemoveEmptyEntries)
                    .Where(x => Regex.Match(x, "\"Value\":\"([^\"]+)\"").Groups[1].Value.ToLower().Contains(searchString.ToLower()));
                return AccountCustomFields.Where(x => data.Any(y => y.ToLower().Contains(string.Format(nameFormat, x.Name).ToLower()) && y.ToLower().Contains(searchString.ToLower())));
            }
            return AccountCustomFields;
        }
    }
}
