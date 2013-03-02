﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Samba.Domain.Models.Accounts;

namespace Samba.Persistance.DaoClasses
{
    public interface IAccountDao
    {
        decimal GetAccountBalance(int accountId);
        decimal GetAccountExchangeBalance(int accountId);
        IEnumerable<Account> GetAccountsByTypeId(int accountTypeId);
        IEnumerable<Account> GetAccountsByIds(IEnumerable<int> accountIds);
        IEnumerable<Account> GetBalancedAccountsByAccountTypeId(int accountTypeId);
        Dictionary<Account, BalanceValue> GetAccountBalances(IList<int> accountTypeIds, Expression<Func<AccountTransactionValue, bool>> filter);
        Dictionary<AccountType, BalanceValue> GetAccountTypeBalances(IList<int> accountTypeIds, Expression<Func<AccountTransactionValue, bool>> filter);
        string GetEntityCustomDataByAccountId(int accountId);
        void CreateAccountTransaction(AccountTransactionType transactionType, Account sourceAccount, Account targetAccount, decimal amount, decimal exchangeRate);
        void CreateTransactionDocument(Account selectedAccount, AccountTransactionDocumentType documentType, string description, decimal amount, decimal exchangeRate, IEnumerable<Account> accounts);
        Account GetAccountById(int accountId);
        bool GetIsAccountNameExists(string accountName);
        int CreateAccount(int accountTypeId, string accountName);
        string GetAccountNameById(int accountId);
        int GetAccountIdByName(string accountName);
        IEnumerable<string> GetAccountNames(Expression<Func<Account, bool>> predictate);
    }
}
