namespace Samba.Domain.Builders
{
    public class AccountTransactionTypeBuilderFor<T> where T : ILinkableToAccountTransactionTypeBuilder<T>
    {
        private readonly T _parent;
        private readonly AccountTransactionTypeBuilder _accountTransactionTypeBuilder;

        public AccountTransactionTypeBuilderFor(T parent)
        {
            _parent = parent;
            _accountTransactionTypeBuilder = new AccountTransactionTypeBuilder();
        }

        public T Do()
        {
            _parent.Link(_accountTransactionTypeBuilder);
            return _parent;
        }

        public AccountTransactionTypeBuilderFor<T> WithId(int id)
        {
            _accountTransactionTypeBuilder.WithId(id);
            return this;
        }
    }
}