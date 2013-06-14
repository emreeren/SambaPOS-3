using Samba.Domain.Models.Accounts;
using Samba.Localization.Properties;
using Samba.Services;

namespace Samba.Modules.AccountModule.Dashboard
{
    internal class AccountTransactionDocumentAccountMapViewModel
    {
        public AccountTransactionDocumentAccountMapViewModel(IAccountService accountService, AccountTransactionDocumentAccountMap accountTransactionDocumentAccountMap, AccountType masterAccountType, AccountType mappingAccountType)
        {
            Model = accountTransactionDocumentAccountMap;
            AccountSelector = new AccountSelectViewModel(accountService, masterAccountType, Model.AccountName, (x, y) => { Model.AccountId = y; Model.AccountName = x; });
            if (mappingAccountType != null)
            {
                MappedAccountSelector = new AccountSelectViewModel(accountService, mappingAccountType, Model.MappedAccountName, (x, y) => { Model.MappedAccountId = y; Model.MappedAccountName = x; });
            }
        }

        public AccountTransactionDocumentAccountMap Model { get; set; }
        public AccountSelectViewModel AccountSelector { get; set; }
        public AccountSelectViewModel MappedAccountSelector { get; set; }
    }
}