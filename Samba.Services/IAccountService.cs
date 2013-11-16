using System;
using System.Collections.Generic;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Settings;
using Samba.Services.Common;

namespace Samba.Services
{
    public interface IAccountService
    {
        decimal GetAccountBalance(int accountId);
        decimal GetAccountExchangeBalance(int accountId);
        IEnumerable<AccountScreenRow> GetAccountScreenRows(AccountScreen accountScreen, WorkPeriod currentWorkPeriod);
        string GetDescription(AccountTransactionDocumentType documentType, Account account);
        decimal GetDefaultAmount(AccountTransactionDocumentType documentType, Account account);
        string GetAccountNameById(int accountId);
        int GetAccountIdByName(string accountName);
        IEnumerable<Account> GetBalancedAccounts(int accountTypeId);
        IEnumerable<string> GetCompletingAccountNames(int accountTypeId, string accountName);
        Account GetAccountById(int accountId);
        Account GetAccountByName(string accountName);
        int CreateAccount(int accountTypeId, string accountName);
        IEnumerable<Account> GetDocumentAccounts(AccountTransactionDocumentType documentType);
        void CreateBatchAccountTransactionDocument(string documentName);
        AccountTransactionDocument CreateTransactionDocument(Account account, AccountTransactionDocumentType documentType, string description, decimal amount, IEnumerable<Account> accounts);
        AccountTransactionDocument GetAccountTransactionDocumentById(int documentId);
        AccountTransactionSummary GetAccountTransactionSummary(Account selectedAccount, WorkPeriod currentWorkPeriod, DateTime? start = null, DateTime? end = null);
        DateRange GetDateRange(string rangeName,WorkPeriod workPeriod);
    }
}
