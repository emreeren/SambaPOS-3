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
        Dictionary<int, decimal> GetAccountBalances(IEnumerable<int> accountIds);
        Dictionary<Account, decimal> GetAccountsWithBalances(IEnumerable<AccountTemplate> accountTemplates);
        string GetDescription(AccountTransactionDocumentTemplate documentTemplate, Account account);
        decimal GetDefaultAmount(AccountTransactionDocumentTemplate documentTemplate, Account account);
        string GetAccountNameById(int accountId);
        int GetAccountIdByName(string accountName);
        IEnumerable<Account> GetAccounts(params AccountTemplate[] accountTemplates);
        IEnumerable<Account> GetAccounts(int accountTemplateId);
        IEnumerable<Account> GetBalancedAccounts(int accountTemplateId);
        IEnumerable<string> GetCompletingAccountNames(int accountTemplateId, string accountName);
        Account GetAccountById(int accountId);
        IEnumerable<AccountTemplate> GetAccountTemplates();
        int CreateAccount(string accountName,int accountTemplateId);
    }
}
