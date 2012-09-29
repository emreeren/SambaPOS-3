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

    public class BalanceValue
    {
        private static BalanceValue _empty;
        public decimal Balance { get; set; }
        public decimal Exchange { get; set; }

        public static BalanceValue Empty
        {
            get
            {
                return _empty ?? (_empty = new BalanceValue());
            }
        }
    }

    public interface IAccountService
    {
        int GetAccountCount();
        void CreateNewTransactionDocument(Account account, AccountTransactionDocumentType DocumentType, string description, decimal amount, IEnumerable<Account> accounts);
        decimal GetAccountBalance(int accountId);
        Dictionary<Account, BalanceValue> GetAccountBalances(IList<int> AccountTypeIds, Expression<Func<AccountTransactionValue, bool>> filter);
        Dictionary<AccountType, BalanceValue> GetAccountTypeBalances(IList<int> AccountTypeIds, Expression<Func<AccountTransactionValue, bool>> filter);
        string GetDescription(AccountTransactionDocumentType DocumentType, Account account);
        decimal GetDefaultAmount(AccountTransactionDocumentType DocumentType, Account account);
        string GetAccountNameById(int accountId);
        int GetAccountIdByName(string accountName);
        IEnumerable<Account> GetAccounts(params AccountType[] AccountTypes);
        IEnumerable<Account> GetAccounts(int AccountTypeId);
        IEnumerable<Account> GetAccounts(IEnumerable<int> accountIds);
        IEnumerable<Account> GetBalancedAccounts(int AccountTypeId);
        IEnumerable<string> GetCompletingAccountNames(int AccountTypeId, string accountName);
        Account GetAccountById(int accountId);
        IEnumerable<AccountType> GetAccountTypes();
        int CreateAccount(string accountName, int AccountTypeId);
        IEnumerable<Account> GetDocumentAccounts(AccountTransactionDocumentType DocumentType);
    }
}
