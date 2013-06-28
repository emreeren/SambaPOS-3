using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Samba.Domain.Models.Accounts;
using Samba.Persistance;
using Samba.Persistance.Common;

namespace Samba.Services
{
    public interface IAccountService
    {
        decimal GetAccountBalance(int accountId);
        Dictionary<Account, BalanceValue> GetAccountBalances(IList<int> accountTypeIds, Expression<Func<AccountTransactionValue, bool>> filter);
        Dictionary<AccountType, BalanceValue> GetAccountTypeBalances(IList<int> accountTypeIds, Expression<Func<AccountTransactionValue, bool>> filter);
        string GetDescription(AccountTransactionDocumentType documentType, Account account);
        decimal GetDefaultAmount(AccountTransactionDocumentType documentType, Account account);
        string GetAccountNameById(int accountId);
        int GetAccountIdByName(string accountName);
        IEnumerable<Account> GetBalancedAccounts(int accountTypeId);
        IEnumerable<string> GetCompletingAccountNames(int accountTypeId, string accountName);
        Account GetAccountById(int accountId);
        int CreateAccount(int accountTypeId, string accountName);
        IEnumerable<Account> GetDocumentAccounts(AccountTransactionDocumentType documentType);
        void CreateBatchAccountTransactionDocument(string documentName);
        AccountTransactionDocument CreateTransactionDocument(Account account, AccountTransactionDocumentType documentType, string description, decimal amount, IEnumerable<Account> accounts);
        AccountTransactionDocument GetAccountTransactionDocumentById(int documentId);
    }
}
