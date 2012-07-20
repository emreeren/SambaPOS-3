using System.Collections.Generic;
using Samba.Domain.Models.Accounts;

namespace Samba.Services
{
    public class AccountData
    {
        public int AccountId { get; set; }
    }

    public interface IAccountService
    {
        int GetAccountCount();
        void CreateNewTransactionDocument(Account account, AccountTransactionDocumentTemplate documentTemplate, string description, decimal amount);
        decimal GetAccountBalance(int accountId);
        string GetDescription(AccountTransactionDocumentTemplate documentTemplate, Account account);
        decimal GetDefaultAmount(AccountTransactionDocumentTemplate documentTemplate, Account account);
        string GetAccountNameById(int accountId);
        int GetAccountIdByName(string accountName);
        IEnumerable<Account> GetAccounts(params AccountTemplate[] accountTemplates);
        IEnumerable<string> GetCompletingAccountNames(int accountTemplateId, string accountName);
        Account GetAccountById(int accountId);
        IEnumerable<AccountTemplate> GetAccountTemplates();
    }
}
