using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using Samba.Domain.Models;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Entities;
using Samba.Domain.Models.Settings;
using Samba.Localization.Properties;
using Samba.Localization;
using Samba.Persistance;
using Samba.Services.Common;

namespace Samba.Services.Implementations.AccountModule
{
    [Export(typeof(IAccountService))]
    public class AccountService : IAccountService
    {
        private readonly ICacheService _cacheService;
        private readonly IAccountDao _accountDao;
        private readonly AccountRowBuilder _accountRowBuilder;

        [ImportingConstructor]
        public AccountService(ICacheService cacheService, IAccountDao accountDao, AccountRowBuilder accountRowBuilder)
        {
            _cacheService = cacheService;
            _accountDao = accountDao;
            _accountRowBuilder = accountRowBuilder;
        }

        public AccountTransactionDocument CreateTransactionDocument(Account selectedAccount, AccountTransactionDocumentType documentType, string description, decimal amount, IEnumerable<Account> accounts)
        {
            var exchangeRate = GetExchangeRate(selectedAccount.ForeignCurrencyId, documentType.ExchangeTemplate);
            var accountData = accounts.Select(x => new AccountData(x) { ExchangeRate = GetExchangeRate(x.ForeignCurrencyId, documentType.ExchangeTemplate) });
            return _accountDao.CreateTransactionDocument(selectedAccount, documentType, description, amount, exchangeRate, accountData, _cacheService.GetForeignCurrencies());
        }

        public AccountTransactionDocument GetAccountTransactionDocumentById(int documentId)
        {
            return _accountDao.GetAccountTransactionDocumentById(documentId);
        }

        public AccountTransactionSummary GetAccountTransactionSummary(Account selectedAccount, WorkPeriod currentWorkPeriod, DateTime? start = null, DateTime? end = null)
        {
            if (!start.HasValue)
            {
                var accountType = _cacheService.GetAccountTypeById(selectedAccount.AccountTypeId);
                if (accountType != null)
                {
                    if (accountType.DefaultFilterType == 1) start = DateTime.Now.MonthStart();
                    if (accountType.DefaultFilterType == 2) start = DateTime.Now.StartOfWeek();
                    if (accountType.DefaultFilterType == 3) start = currentWorkPeriod.StartDate;
                }
            }
            return
                    AccountTransactionSummaryBuilder.Create()
                                                    .ForAccount(selectedAccount)
                                                    .WithStartDate(start)
                                                    .WithEndDate(end)
                                                    .Build();
        }

        public DateRange GetDateRange(string rangeName, WorkPeriod workPeriod)
        {
            if (rangeName == Resources.ThisMonth) return new DateRange(DateTime.Now.MonthStart(), null);
            if (rangeName == Resources.PastMonth) return new DateRange(DateTime.Now.AddMonths(-1).MonthStart(), DateTime.Now.AddMonths(-1).MonthEnd());
            if (rangeName == Resources.ThisWeek) return new DateRange(DateTime.Now.StartOfWeek(), null);
            if (rangeName == Resources.PastWeek) return new DateRange(DateTime.Now.StartOfPastWeek(), DateTime.Now.StartOfWeek().AddDays(7));
            if (rangeName == Resources.WorkPeriod) return new DateRange(workPeriod.StartDate, null);
            return new DateRange(null, null);
        }

        public decimal GetAccountBalance(int accountId)
        {
            return _accountDao.GetAccountBalance(accountId);
        }

        public decimal GetAccountExchangeBalance(int accountId)
        {
            return _accountDao.GetAccountExchangeBalance(accountId);
        }

        public IEnumerable<AccountScreenRow> GetAccountScreenRows(AccountScreen accountScreen, WorkPeriod currentWorkPeriod)
        {
            return _accountRowBuilder.GetAccountScreenRows(accountScreen, currentWorkPeriod);
        }

        public string GetCustomData(Account account, string fieldName)
        {
            var cd = _accountDao.GetEntityCustomDataByAccountId(account.Id);
            return string.IsNullOrEmpty(cd) ? "" : Entity.GetCustomData(cd, fieldName);
        }

        public string GetDescription(AccountTransactionDocumentType documentType, Account account)
        {
            var result = documentType.DescriptionTemplate;
            if (string.IsNullOrEmpty(result)) return result;
            while (Regex.IsMatch(result, "\\[:([^\\]]+)\\]"))
            {
                var match = Regex.Match(result, "\\[:([^\\]]+)\\]");
                result = result.Replace(match.Groups[0].Value, GetCustomData(account, match.Groups[1].Value));
            }
            if (result.Contains("[MONTH]")) result = result.Replace("[MONTH]", DateTime.Now.ToMonthName());
            if (result.Contains("[NEXT MONTH]")) result = result.Replace("[NEXT MONTH]", DateTime.Now.ToNextMonthName());
            if (result.Contains("[WEEK]")) result = result.Replace("[WEEK]", DateTime.Now.WeekOfYear().ToString(CultureInfo.CurrentCulture));
            if (result.Contains("[NEXT WEEK]")) result = result.Replace("[NEXT WEEK]", (DateTime.Now.NextWeekOfYear()).ToString(CultureInfo.CurrentCulture));
            if (result.Contains("[YEAR]")) result = result.Replace("[YEAR]", (DateTime.Now.Year).ToString(CultureInfo.CurrentCulture));
            if (result.Contains("[ACCOUNT NAME]")) result = result.Replace("[ACCOUNT NAME]", account.Name);
            return result;
        }

        public decimal GetDefaultAmount(AccountTransactionDocumentType documentType, Account account)
        {
            decimal result = 0;
            if (!string.IsNullOrEmpty(documentType.DefaultAmount))
            {
                var da = documentType.DefaultAmount;
                if (Regex.IsMatch(da, "\\[:([^\\]]+)\\]"))
                {
                    var match = Regex.Match(da, "\\[:([^\\]]+)\\]");
                    da = GetCustomData(account, match.Groups[1].Value);
                    decimal.TryParse(da, out result);
                }
                else if (da == string.Format("[{0}]", Resources.Balance))
                {
                    result = Math.Abs(GetAccountBalance(account.Id));
                }
                else if (da == string.Format("[{0}]", "BalanceEx"))
                {
                    var er = GetExchangeRate(account.ForeignCurrencyId, documentType.ExchangeTemplate);
                    result = er != 1
                        ? Math.Abs(GetAccountExchangeBalance(account.Id) * er)
                        : Math.Abs(GetAccountBalance(account.Id));
                }
                else decimal.TryParse(da, out result);
            }
            return result;
        }

        public string GetAccountNameById(int accountId)
        {
            return _accountDao.GetAccountNameById(accountId);
        }

        public int GetAccountIdByName(string accountName)
        {
            return _accountDao.GetAccountIdByName(accountName);
        }

        public IEnumerable<Account> GetAccounts(int accountTypeId)
        {
            return _accountDao.GetAccountsByTypeId(accountTypeId);
        }

        public IEnumerable<Account> GetAccounts(IEnumerable<int> accountIds)
        {
            return _accountDao.GetAccountsByIds(accountIds);
        }

        public IEnumerable<Account> GetBalancedAccounts(int accountTypeId)
        {
            return _accountDao.GetBalancedAccountsByAccountTypeId(accountTypeId);
        }

        public IEnumerable<string> GetCompletingAccountNames(int accountTypeId, string accountName)
        {
            if (string.IsNullOrWhiteSpace(accountName)) return null;
            var lacn = accountName.ToLower();
            Expression<Func<Account, bool>> predictate = x => x.AccountTypeId == accountTypeId && x.Name.ToLower().Contains(lacn);
            return _accountDao.GetAccountNames(predictate);
        }

        public Account GetAccountById(int accountId)
        {
            return _accountDao.GetAccountById(accountId);
        }

        public Account GetAccountByName(string accountName)
        {
            return _accountDao.GetAccountByName(accountName);
        }

        public int CreateAccount(int accountTypeId, string accountName)
        {
            if (accountTypeId == 0 || string.IsNullOrEmpty(accountName)) return 0;
            if (_accountDao.GetIsAccountNameExists(accountName)) return 0;
            return _accountDao.CreateAccount(accountTypeId, accountName);
        }

        public IEnumerable<Account> GetDocumentAccounts(AccountTransactionDocumentType documentType)
        {
            switch (documentType.Filter)
            {
                case 1: return GetBalancedAccounts(documentType.MasterAccountTypeId);
                case 2: return GetAccounts(documentType.AccountTransactionDocumentAccountMaps.Select(x => x.AccountId));
                default: return GetAccounts(documentType.MasterAccountTypeId);
            }
        }

        public void CreateBatchAccountTransactionDocument(string documentName)
        {
            if (!string.IsNullOrEmpty(documentName))
            {
                var document = _cacheService.GetAccountTransactionDocumentTypeByName(documentName);
                if (document != null)
                {
                    var accounts = GetDocumentAccounts(document);
                    foreach (var account in accounts)
                    {
                        var map = document.AccountTransactionDocumentAccountMaps.FirstOrDefault(y => y.AccountId == account.Id);
                        if (map != null && map.MappedAccountId > 0)
                        {
                            var targetAccount = new Account { Id = map.MappedAccountId, Name = map.MappedAccountName };
                            var amount = GetDefaultAmount(document, account);
                            if (amount != 0)
                            {
                                CreateTransactionDocument(account, document, "", amount, new List<Account> { targetAccount });
                            }
                        }
                    }
                }
            }
        }

        public decimal GetExchangeRate(int foreignCurrencyId, string template)
        {
            if (foreignCurrencyId == 0) return 1;
            if (!string.IsNullOrWhiteSpace(template))
            {
                decimal d;
                decimal.TryParse(template, out d);
                return d;
            }
            return _cacheService.GetCurrencyById(foreignCurrencyId).ExchangeRate;
        }
    }
}
