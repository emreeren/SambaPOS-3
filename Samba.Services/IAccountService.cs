using Samba.Domain.Models.Accounts;

namespace Samba.Services
{
    public interface IAccountService
    {
        int GetAccountCount();
        void CreateNewTransactionDocument(Account account, AccountTransactionDocumentTemplate documentTemplate, string description, decimal amount);
        decimal GetAccountBalance(int accountId);
        string GetDescription(AccountTransactionDocumentTemplate documentTemplate, Account account);
        decimal GetDefaultAmount(AccountTransactionDocumentTemplate documentTemplate, Account account);
    }
}
