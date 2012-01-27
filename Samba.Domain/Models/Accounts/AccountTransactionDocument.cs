using System.Collections.Generic;
using System.Linq;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Accounts
{
    public class AccountTransactionDocument : IEntity
    {
        public AccountTransactionDocument()
        {
            _accountTransactionDocumentLines = new List<AccountTransactionDocumentLine>();
        }

        public int Id { get; set; }
        public string Name { get; set; }

        public virtual AccountTransactionDocumentTemplate AccountTransactionDocumentTemplate { get; set; }
        public virtual AccountTransactionDocumentLine SourceTransactionLine { get; set; }

        private IList<AccountTransactionDocumentLine> _accountTransactionDocumentLines;
        public virtual IList<AccountTransactionDocumentLine> AccountTransactionDocumentLines
        {
            get { return _accountTransactionDocumentLines; }
            set { _accountTransactionDocumentLines = value; }
        }

        public static AccountTransactionDocument Create(AccountTransactionDocumentTemplate template)
        {
            var result = new AccountTransactionDocument
                             {
                                 AccountTransactionDocumentTemplate = new AccountTransactionDocumentTemplate(),
                                 SourceTransactionLine = new AccountTransactionDocumentLine(),
                             };

            result.SourceTransactionLine.SourceTransaction.Account = template.SourceAccount;
            result.SourceTransactionLine.TargetTransaction.Account = template.TargetAccount;
            return result;
        }

        public AccountTransactionDocumentLine AddLine(string description, Account transactionAccount, decimal amount)
        {
            var result = new AccountTransactionDocumentLine();
            result.SourceTransaction.Account = AccountTransactionDocumentTemplate.TargetAccount;
            result.TargetTransaction.Account = transactionAccount ?? AccountTransactionDocumentTemplate.TransactionAccount;
            result.Amount = amount;
            AccountTransactionDocumentLines.Add(result);
            Recalculate();
            return result;
        }

        public void Recalculate()
        {
            SourceTransactionLine.Amount = AccountTransactionDocumentLines.Sum(x => x.Amount);
        }
    }
}
