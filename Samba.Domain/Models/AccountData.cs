using Samba.Domain.Models.Accounts;

namespace Samba.Domain.Models
{
    public class AccountData
    {
        private AccountData()
        {
            ExchangeRate = 1;
        }

        public AccountData(int accountId)
            : this()
        {
            AccountId = accountId;
        }

        public AccountData(int accountTypeId, int accountId)
            : this()
        {
            AccountTypeId = accountTypeId;
            AccountId = accountId;
        }

        public AccountData(Account account)
            : this()
        {
            AccountTypeId = account.AccountTypeId;
            AccountId = account.Id;
        }

        public int AccountTypeId { get; set; }
        public int AccountId { get; set; }
        public decimal ExchangeRate { get; set; }
    }
}