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
            _accountTransactionTemplate =
                AccountTransactionTemplates.SingleOrDefault(x => x.Id == Model.AccountTransactionTemplateId);
        }

        private IEnumerable<AccountTransactionTemplate> _accountTransactionTemplates;
        public IEnumerable<AccountTransactionTemplate> AccountTransactionTemplates
        {
            get { return _accountTransactionTemplates ?? (_accountTransactionTemplates = _workspace.All<AccountTransactionTemplate>().ToList()); }
        }

        private IEnumerable<Account> _sourceAccounts;
        public IEnumerable<Account> SourceAccounts
        {
            get
            {
                if (AccountTransactionTemplate == null) return new List<Account>();
                return _sourceAccounts ?? (_sourceAccounts = _workspace.All<Account>(x => x.AccountTemplateId == Model.SourceAccountTemplateId).ToList());
            }
        }

        private IEnumerable<Account> _targetAccounts;
        public IEnumerable<Account> TargetAccounts
        {
            get
            {
                if (AccountTransactionTemplate == null) return new List<Account>();
                return _targetAccounts ?? (_targetAccounts = _workspace.All<Account>(x => x.AccountTemplateId == Model.TargetAccountTemplateId).ToList());
            }
        }

        private AccountTransactionTemplate _accountTransactionTemplate;
        public AccountTransactionTemplate AccountTransactionTemplate
        {
            get { return _accountTransactionTemplate; }
            set
            {
                _accountTransactionTemplate = value;
                if (Model == AccountTransaction.Null)
                {
                    Model = AccountTransaction.Create(value);
                    Model.SourceAccountTemplateId = value.SourceAccountTemplateId;
                    Model.TargetAccountTemplateId = value.TargetAccountTemplateId;
                    _document.AccountTransactions.Add(Model);
                }
                RaisePropertyChanged(() => AccountTransactionTemplate);
                RaisePropertyChanged(() => SourceAccount);
                RaisePropertyChanged(() => TargetAccount);
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
