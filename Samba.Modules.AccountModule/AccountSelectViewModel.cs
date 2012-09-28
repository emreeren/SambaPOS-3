using System;
using System.Collections.Generic;
using System.Linq;
using Samba.Domain.Models.Accounts;
using Samba.Presentation.Common;
using Samba.Services;

namespace Samba.Modules.AccountModule
{
    public class AccountSelectViewModel : ObservableObject
    {
        private readonly IAccountService _accountService;

        public AccountSelectViewModel(IAccountService accountService, AccountTemplate accountTemplate)
        {
            _accountService = accountService;
            AccountTemplate = accountTemplate;
        }

        public AccountSelectViewModel(IAccountService accountService, AccountTemplate accountTemplate, int accountId, string accountName)
            : this(accountService, accountTemplate)
        {
            _accountName = accountName;
            _selectedAccountId = accountId;
        }

        public AccountSelectViewModel(IAccountService accountService, AccountTemplate accountTemplate, string accountName, Action<string, int> updateAction)
            : this(accountService, accountTemplate)
        {
            _accountName = accountName;
            UpdateAction = updateAction;
        }

        protected Action<string, int> UpdateAction { get; set; }

        private string _accountName;
        public string AccountName
        {
            get { return _accountName ?? (_accountName = _accountService.GetAccountNameById(SelectedAccountId)); }
            set
            {
                _accountName = value;
                SelectedAccountId = _accountService.GetAccountIdByName(value);
                if (SelectedAccountId == 0)
                    RaisePropertyChanged(() => AccountNames);
                _accountName = null;
                RaisePropertyChanged(() => AccountName);
            }
        }

        public IEnumerable<string> AccountNames
        {
            get
            {
                if (AccountTemplate == null) return null;
                return _accountService.GetCompletingAccountNames(AccountTemplate.Id, AccountName);
            }
        }

        private AccountTemplate _accountTemplate;
        private int _selectedAccountId;

        public AccountTemplate AccountTemplate
        {
            get { return _accountTemplate; }
            set
            {
                _accountTemplate = value;
                RaisePropertyChanged(() => AccountTemplate);
                RaisePropertyChanged(() => TemplateName);
            }
        }

        public int SelectedAccountId
        {
            get { return _selectedAccountId; }
            set
            {
                _selectedAccountId = value;
                if (UpdateAction != null)
                    UpdateAction.Invoke(AccountName, value);
            }
        }

        public string TemplateName { get { return AccountTemplate == null ? "" : string.Format("{0}:", AccountTemplate.Name); } }
    }
}