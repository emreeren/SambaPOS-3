using Samba.Domain.Models.Accounts;

namespace Samba.Domain.Builders
{
    public class AccountTransactionTypeBuilder
    {
        private int _id;

        public static AccountTransactionTypeBuilder Create()
        {
            return new AccountTransactionTypeBuilder();
        }

        public AccountTransactionTypeBuilder WithId(int id)
        {
            _id = id;
            return this;
        }

        public AccountTransactionType Build()
        {
            var result = new AccountTransactionType { Id = _id };
            return result;
        }
    }
}