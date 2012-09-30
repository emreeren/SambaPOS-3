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
        IEnumerable<AccountType> GetAccountTypes();
        int CreateAccount(int accountTypeId, string accountName);
        IEnumerable<Account> GetDocumentAccounts(AccountTransactionDocumentType documentType);
        void CreateBatchAccountTransactionDocument(string documentName);
        void CreateTransactionDocument(Account account, AccountTransactionDocumentType documentType, string description, decimal amount, IEnumerable<Account> accounts);
    }
}
