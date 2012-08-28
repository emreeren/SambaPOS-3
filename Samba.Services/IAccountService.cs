using System;
using System.Collections.Generic;
using System.Linq.Expressions;
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
        void CreateNewTransactionDocument(Account account, AccountTransactionDocumentTemplate documentTemplate, string description, decimal amount, IEnumerable<Account> accounts);
        decimal GetAccountBalance(int accountId);
        Dictionary<Account, decimal> GetAccountBalances(IList<int> accountTemplateIds, Expression<Func<AccountTransactionValue, bool>> filter);
        Dictionary<AccountTemplate, decimal> GetAccountTemplateBalances(IList<int> accountTemplateIds, Expression<Func<AccountTransactionValue, bool>> filter);
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
