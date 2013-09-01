using System.Collections.Generic;
using System.Linq;
using Samba.Domain.Models.Accounts;
using Samba.Infrastructure.Data;
using Samba.Presentation.Common;

namespace Samba.Modules.AccountModule.Dashboard
{
    class AccountTransactionViewModel : ObservableObject
    {
        private readonly IWorkspace _workspace;
        private readonly AccountTransactionDocument _document;
        public AccountTransaction Model { get; set; }

        public AccountTransactionViewModel(IWorkspace workspace, AccountTransaction model, AccountTransactionDocument document)
        {
            Model = model ?? AccountTransaction.Null;
            _document = document;
            _workspace = workspace;
            _accountTransactionType =
                AccountTransactionTypes.SingleOrDefault(x => x.Id == Model.AccountTransactionTypeId);
        }

        private IEnumerable<AccountTransactionType> _accountTransactionTypes;
        public IEnumerable<AccountTransactionType> AccountTransactionTypes
        {
            get { return _accountTransactionTypes ?? (_accountTransactionTypes = _workspace.All<AccountTransactionType>().ToList()); }
        }

        private IEnumerable<Account> _sourceAccounts;
        public IEnumerable<Account> SourceAccounts
        {
            get
            {
                if (AccountTransactionType == null) return new List<Account>();
                return _sourceAccounts ?? (_sourceAccounts = _workspace.All<Account>(x => x.AccountTypeId == Model.SourceAccountTypeId).ToList());
            }
        }

        private IEnumerable<Account> _targetAccounts;
        public IEnumerable<Account> TargetAccounts
        {
            get
            {
                if (AccountTransactionType == null) return new List<Account>();
                return _targetAccounts ?? (_targetAccounts = _workspace.All<Account>(x => x.AccountTypeId == Model.TargetAccountTypeId).ToList());
            }
        }

        private AccountTransactionType _accountTransactionType;
        public AccountTransactionType AccountTransactionType
        {
            get { return _accountTransactionType; }
            set
            {
                _accountTransactionType = value;
                if (Model == AccountTransaction.Null)
                {
                    Model = AccountTransaction.Create(value);
                    _document.AccountTransactions.Add(Model);
                }
                else if(Model.AccountTransactionTypeId != value.Id)
                {
                    if (_document.AccountTransactions.Contains(Model))
                        _document.AccountTransactions.Remove(Model);
                    Model = AccountTransaction.Null;
                    _sourceAccounts = null;
                    _targetAccounts = null;
                    AccountTransactionType = value;
                }
                RaisePropertyChanged(() => AccountTransactionType);
                RaisePropertyChanged(() => SourceAccount);
                RaisePropertyChanged(() => TargetAccount);
                RaisePropertyChanged(() => SourceAccounts);
                RaisePropertyChanged(() => TargetAccounts);
            }
        }

        public Account SourceAccount
        {
            get { return SourceAccounts.SingleOrDefault(x => x.Id == SourceAccountId); }
            set
            {
                SourceAccountId = value.Id;
                RaisePropertyChanged(() => SourceAccount);
            }
        }

        public Account TargetAccount
        {
            get { return TargetAccounts.SingleOrDefault(x => x.Id == TargetAccountId); }
            set
            {
                TargetAccountId = value.Id;
                RaisePropertyChanged(() => TargetAccount);
            }
        }

        public int SourceAccountId { get { return Model.SourceTransactionValue.AccountId; } set { Model.SourceTransactionValue.AccountId = value; } }
        public int TargetAccountId { get { return Model.TargetTransactionValue.AccountId; } set { Model.TargetTransactionValue.AccountId = value; } }

        public string Name { get { return Model.Name; } set { Model.Name = value; } }

        public decimal Amount
        {
            get { return Model.Amount; }
            set { Model.Amount = value; }
        }
    }
}
