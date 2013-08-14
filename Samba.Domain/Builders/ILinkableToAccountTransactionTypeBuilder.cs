namespace Samba.Domain.Builders
{
    public interface ILinkableToAccountTransactionTypeBuilder<T> where T : ILinkableToAccountTransactionTypeBuilder<T>
    {
        void Link(AccountTransactionTypeBuilder accountTransactionTypeBuilder);
        AccountTransactionTypeBuilderFor<T> CreateAccountTransactionType();
    }
}