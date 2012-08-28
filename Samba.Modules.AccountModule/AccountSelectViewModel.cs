using System.Collections.Generic;
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

        public int SelectedAccountId { get; set; }
        public string TemplateName { get { return AccountTemplate == null ? "" : string.Format("{0}:", AccountTemplate.Name); } }
    }
}