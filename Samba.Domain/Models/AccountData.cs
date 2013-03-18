using Samba.Domain.Models.Accounts;

namespace Samba.Domain.Models
{
    public class AccountData
    {
        public AccountData(int accountId)
        {
            AccountId = accountId;
        }
        
        public AccountData(int accountTypeId, int accountId)
        {
            AccountTypeId = accountTypeId;
            AccountId = accountId;
        }

        public AccountData(Account account)
        {
            AccountTypeId = account.AccountTypeId;
            AccountId = account.Id;
        }

        public int AccountTypeId { get; set; }
        public int AccountId { get; set; }
    }
}