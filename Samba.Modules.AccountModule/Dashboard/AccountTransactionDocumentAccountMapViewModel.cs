using Samba.Domain.Models.Accounts;
using Samba.Services;

namespace Samba.Modules.AccountModule.Dashboard
{
    internal class AccountTransactionDocumentAccountMapViewModel
    {
        public AccountTransactionDocumentAccountMapViewModel(IAccountService accountService, AccountTransactionDocumentAccountMap accountTransactionDocumentAccountMap, AccountTemplate masterAccountTemplate, AccountTemplate mappingAccountTemplate)
        {
            Model = accountTransactionDocumentAccountMap;
            AccountSelector = new AccountSelectViewModel(accountService, masterAccountTemplate, Model.AccountName, (x, y) => { Model.AccountId = y; Model.AccountName = x; });
            if (mappingAccountTemplate != null)
            {
                MappedAccountSelector = new AccountSelectViewModel(accountService, mappingAccountTemplate, Model.MappedAccountName, (x, y) => { Model.MappedAccountId = y; Model.MappedAccountName = x; });
            }
        }

        public AccountTransactionDocumentAccountMap Model { get; set; }
        public AccountSelectViewModel AccountSelector { get; set; }
        public AccountSelectViewModel MappedAccountSelector { get; set; }
    }
}