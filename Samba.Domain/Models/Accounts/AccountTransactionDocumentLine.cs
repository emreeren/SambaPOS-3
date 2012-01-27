using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Accounts
{
    public class AccountTransactionDocumentLine : IEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public int AccountTransactionDocumentId { get; set; }

        private decimal _amount;
        public decimal Amount
        {
            get { return _amount; }
            set
            {
                _amount = value;
                SourceTransaction.Receivable = value;
                TargetTransaction.Liability = value;
            }
        }

        private AccountTransaction _sourceTransaction;
        public virtual AccountTransaction SourceTransaction
        {
            get { return _sourceTransaction ?? (_sourceTransaction = new AccountTransaction()); }
            set { _sourceTransaction = value; }
        }

        private AccountTransaction _targetTransaction;
        public virtual AccountTransaction TargetTransaction
        {
            get { return _targetTransaction ?? (_targetTransaction = new AccountTransaction()); }
            set { _targetTransaction = value; }
        }
    }
}
