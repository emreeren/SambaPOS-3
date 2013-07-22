using System.Collections.Generic;
using System.Linq;
using Samba.Domain.Models.Accounts;
using Samba.Presentation.Common;
using Samba.Services;

namespace Samba.Modules.AccountModule
{
    public class AccountRowViewModel : ObservableObject
    {
        private readonly Account _account;

        public AccountRowViewModel(Account account, AccountTransactionDocumentType documentType, IAccountService accountService, ICacheService cacheService)
        {
            _account = account;
            Amount = accountService.GetDefaultAmount(documentType, account);
            Description = accountService.GetDescription(documentType, account);
            TargetAccounts = GetAccountSelectors(documentType, account, accountService, cacheService).ToList();
        }

        public AccountSelectViewModel this[int accountTypeId] { get { return TargetAccounts.Single(x => x.AccountType.Id == accountTypeId); } }

        public Account Account
        {
            get { return _account; }
        }

        public IList<AccountSelectViewModel> TargetAccounts { get; set; }

        private string _description;
        public string Description
        {
            get { return _description; }
            set
            {
                _description = value;
                RaisePropertyChanged(() => Description);
            }
        }

        private decimal _amount;
        public decimal Amount
        {
            get { return _amount; }
            set
            {
                _amount = value;
                IsSelected = _amount != 0;
                RaisePropertyChanged(() => Amount);
            }
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                RaisePropertyChanged(() => IsSelected);
            }
        }

        private IEnumerable<AccountSelectViewModel> GetAccountSelectors(AccountTransactionDocumentType documentType, Account selectedAccount, IAccountService accountService, ICacheService cacheService)
        {
            var accountMap = documentType.AccountTransactionDocumentAccountMaps.FirstOrDefault(x => x.AccountId == selectedAccount.Id);
            return accountMap != null
                       ? documentType.GetNeededAccountTypes().Select(x => new AccountSelectViewModel(accountService, cacheService.GetAccountTypeById(x), accountMap.MappedAccountId, accountMap.MappedAccountName))
                       : documentType.GetNeededAccountTypes().Select(x => new AccountSelectViewModel(accountService, cacheService.GetAccountTypeById(x)));
        }
    }
}