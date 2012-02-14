using System;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using Samba.Domain.Models.Accounts;
using Samba.Presentation.Common;
using Samba.Presentation.Common.ModelBase;

namespace Samba.Modules.AccountModule.Dashboard
{
    [Export, PartCreationPolicy(CreationPolicy.NonShared)]
    class AccountTransactionDocumentViewModel : EntityViewModelBase<AccountTransactionDocument>
    {
        public AccountTransactionDocumentViewModel()
        {
            AddItemCommand = new CaptionCommand<string>("Add Item", OnAddItem);
        }

        public ICaptionCommand AddItemCommand { get; set; }

        private ObservableCollection<AccountTransactionViewModel> _accountTransactions;
        public ObservableCollection<AccountTransactionViewModel> AccountTransactions
        {
            get { return _accountTransactions ?? (_accountTransactions = CreateAccountTransactions()); }
        }

        private ObservableCollection<AccountTransactionViewModel> CreateAccountTransactions()
        {
            var result = new ObservableCollection<AccountTransactionViewModel>();
            result.AddRange(Model.AccountTransactions.Select(x => new AccountTransactionViewModel(Workspace, x, Model)));
            return result;
        }

        private void OnAddItem(string obj)
        {
            var transaction = new AccountTransactionViewModel(Workspace, null, Model);
            AccountTransactions.Add(transaction);
        }

        public override Type GetViewType()
        {
            return typeof(AccountTransactionDocumentView);
        }

        public override string GetModelTypeString()
        {
            return "Account Transaction Document";
        }
    }
}
