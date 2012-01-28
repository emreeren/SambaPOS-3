using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Accounts
{
    public class AccountTransactionDocument : IEntity
    {
        public AccountTransactionDocument()
        {
            _accountTransactions = new List<AccountTransaction>();
        }

        public int Id { get; set; }
        public string Name { get; set; }

        private IList<AccountTransaction> _accountTransactions;
        public virtual IList<AccountTransaction> AccountTransactions
        {
            get { return _accountTransactions; }
            set { _accountTransactions = value; }
        }
    }
}
