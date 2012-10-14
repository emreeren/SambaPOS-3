using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Resources;
using Samba.Infrastructure.Data;
using Samba.Localization.Properties;
using Samba.Persistance.Data;
using Samba.Services.Common;
using Samba.Persistance.Data.Specification;

namespace Samba.Services.Implementations.AccountModule
{
    [Export(typeof(IAccountService))]
    public class AccountService : AbstractService, IAccountService
    {
        private readonly ICacheService _cacheService;

        [ImportingConstructor]
        public AccountService(ICacheService cacheService)
        {
            _cacheService = cacheService;
            ValidatorRegistry.RegisterDeleteValidator(new AccountDeleteValidator());
            ValidatorRegistry.RegisterDeleteValidator(new AccountTypeDeleteValidator());
            ValidatorRegistry.RegisterDeleteValidator<AccountTransactionType>(x => Dao.Exists<AccountTransactionDocumentType>(y => y.TransactionTypes.Any(z => z.Id == x.Id)), Resources.AccountTransactionType, Resources.DocumentType);
            ValidatorRegistry.RegisterSaveValidator(new NonDuplicateSaveValidator<Account>(string.Format(Resources.SaveErrorDuplicateItemName_f, Resources.Account)));
            ValidatorRegistry.RegisterSaveValidator(new NonDuplicateSaveValidator<AccountType>(string.Format(Resources.SaveErrorDuplicateItemName_f, Resources.AccountType)));
            ValidatorRegistry.RegisterSaveValidator(new NonDuplicateSaveValidator<AccountTransactionType>(string.Format(Resources.SaveErrorDuplicateItemName_f, Resources.AccountTransactionType)));
            ValidatorRegistry.RegisterSaveValidator(new NonDuplicateSaveValidator<AccountTransactionDocumentType>(string.Format(Resources.SaveErrorDuplicateItemName_f, Resources.DocumentType)));
        }

        public void CreateTransactionDocument(Account selectedAccount, AccountTransactionDocumentType documentType, string description, decimal amount, IEnumerable<Account> accounts)
        {
            using (var w = WorkspaceFactory.Create())
            {
                var document = documentType.CreateDocument(selectedAccount, description, amount, GetExchangeRate(selectedAccount), accounts != null ? accounts.ToList() : null);
                w.Add(document);
                w.CommitChanges();
            }
        }

        public void CreateAccountTransaction(Account sourceAccount, Account targetAccount, decimal amount, decimal exchangeRate)
        {
            var transactionType = _cacheService.FindAccountTransactionType(sourceAccount.AccountTypeId, targetAccount.AccountTypeId);
            if (transactionType != null)
            {
                using (var w = WorkspaceFactory.Create())
                {
                    var doc = new AccountTransactionDocument();
                    w.Add(doc);
                    doc.AddNewTransaction(transactionType, sourceAccount.AccountTypeId, sourceAccount.Id, targetAccount, amount, exchangeRate);
                    w.CommitChanges();
                }
            }
        }

        public decimal GetAccountBalance(int accountId)
        {
            return Dao.Sum<AccountTransactionValue>(x => x.Debit - x.Credit, x => x.AccountId == accountId);
        }

        public Dictionary<Account, BalanceValue> GetAccountBalances(IList<int> accountTypeIds, Expression<Func<AccountTransactionValue, bool>> filter)
        {
            using (var w = WorkspaceFactory.CreateReadOnly())
            {
                var accountIds = w.Queryable<Account>().Where(x => accountTypeIds.Contains(x.AccountTypeId)).Select(x => x.Id);
                Expression<Func<AccountTransactionValue, bool>> func = x => accountIds.Contains(x.AccountId);
                if (filter != null) func = func.And(filter);
                var transactionValues = w.Queryable<AccountTransactionValue>()
                    .Where(func)
                    .GroupBy(x => x.AccountId)
                    .Select(x => new { Id = x.Key, Amount = x.Sum(y => y.Debit - y.Credit), Exchange = x.Sum(y => y.Exchange) })
                    .ToDictionary(x => x.Id, x => new BalanceValue { Balance = x.Amount, Exchange = x.Exchange });

                return w.Queryable<Account>().Where(x => accountTypeIds.Contains(x.AccountTypeId)).ToDictionary(x => x, x => transactionValues.ContainsKey(x.Id) ? transactionValues[x.Id] : BalanceValue.Empty);
            }
        }

        public Dictionary<AccountType, BalanceValue> GetAccountTypeBalances(IList<int> accountTypeIds, Expression<Func<AccountTransactionValue, bool>> filter)
        {
            using (var w = WorkspaceFactory.CreateReadOnly())
            {
                Expression<Func<AccountTransactionValue, bool>> func = x => accountTypeIds.Contains(x.AccountTypeId);
                if (filter != null) func = func.And(filter);
                var transactionValues = w.Queryable<AccountTransactionValue>()
                    .Where(func)
                    .GroupBy(x => x.AccountTypeId)
                    .Select(x => new { Id = x.Key, Amount = x.Sum(y => y.Debit - y.Credit), Exchange = x.Sum(y => y.Exchange) })
                    .ToDictionary(x => x.Id, x => new BalanceValue { Balance = x.Amount, Exchange = x.Exchange });
                return w.Queryable<AccountType>().Where(x => accountTypeIds.Contains(x.Id)).ToDictionary(x => x, x => transactionValues.ContainsKey(x.Id) ? transactionValues[x.Id] : BalanceValue.Empty);
            }
        }

        public string GetCustomData(Account account, string fieldName)
        {
            var cd = Dao.Select<Resource, string>(x => x.CustomData, x => x.AccountId == account.Id).SingleOrDefault();
            return string.IsNullOrEmpty(cd) ? "" : Resource.GetCustomData(cd, fieldName);
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
                    result = Math.Abs(GetAccountBalance(account.Id));
                else decimal.TryParse(da, out result);
            }
            return result;
        }

        public string GetAccountNameById(int accountId)
        {
            return Dao.Exists<Account>(x => x.Id == accountId)
                ? Dao.Select<Account, string>(x => x.Name, x => x.Id == accountId).First() : "";
        }

        public int GetAccountIdByName(string accountName)
        {
            var acName = accountName.ToLower();
            return Dao.Exists<Account>(x => x.Name.ToLower() == acName)
                ? Dao.Select<Account, int>(x => x.Id, x => x.Name.ToLower() == acName).FirstOrDefault() : 0;
        }

        public IEnumerable<Account> GetAccounts(int accountTypeId)
        {
            return Dao.Query<Account>(x => x.AccountTypeId == accountTypeId);
        }

        public IEnumerable<Account> GetAccounts(IEnumerable<int> accountIds)
        {
            var ids = accountIds.ToList();
            return ids.Any() ? Dao.Query<Account>(x => ids.Contains(x.Id)) : new List<Account>();
        }

        public IEnumerable<Account> GetBalancedAccounts(int accountTypeId)
        {
            using (var w = WorkspaceFactory.CreateReadOnly())
            {
                var q1 = w.Queryable<AccountTransactionValue>().GroupBy(x => x.AccountId).Where(
                        x => x.Sum(y => y.Debit - y.Credit) != 0).Select(x => x.Key);
                return w.Queryable<Account>().Where(x => x.AccountTypeId == accountTypeId && q1.Contains(x.Id)).ToList();
            }
        }

        public IEnumerable<string> GetCompletingAccountNames(int accountTypeId, string accountName)
        {
            if (string.IsNullOrWhiteSpace(accountName)) return null;
            var lacn = accountName.ToLower();
            return Dao.Select<Account, string>(x => x.Name, x => x.AccountTypeId == accountTypeId && x.Name.ToLower().Contains(lacn));
        }

        public Account GetAccountById(int accountId)
        {
            return Dao.Single<Account>(x => x.Id == accountId);
        }

        public IEnumerable<AccountType> GetAccountTypes()
        {
            return Dao.Query<AccountType>().OrderBy(x => x.Order);
        }

        public int CreateAccount(int accountTypeId, string accountName)
        {
            if (accountTypeId == 0 || string.IsNullOrEmpty(accountName)) return 0;
            if (Dao.Exists<Account>(x => x.Name == accountName)) return 0;
            using (var w = WorkspaceFactory.Create())
            {
                var account = new Account { AccountTypeId = accountTypeId, Name = accountName };
                w.Add(account);
                w.CommitChanges();
                return account.Id;
            }
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
                        var map = document.AccountTransactionDocumentAccountMaps.FirstOrDefault(
                            y => y.AccountId == account.Id);
                        if (map != null && map.MappedAccountId > 0)
                        {
                            var targetAccount = new Account { Id = map.MappedAccountId, Name = map.MappedAccountName };
                            var amount = GetDefaultAmount(document, account);
                            if (amount != 0)
                                CreateTransactionDocument(account, document, "", amount,
                                                                             new List<Account> { targetAccount });
                        }
                    }
                }
            }
        }

        public decimal GetExchangeRate(Account account)
        {
            if (account.ForeignCurrencyId == 0) return 1;
            return _cacheService.GetForeignCurrencies().Single(x => x.Id == account.ForeignCurrencyId).ExchangeRate;
        }

        public override void Reset()
        {

        }
    }

    public class AccountTypeDeleteValidator : SpecificationValidator<AccountType>
    {
        public override string GetErrorMessage(AccountType model)
        {
            if (Dao.Exists<Account>(x => x.AccountTypeId == model.Id))
                return string.Format(Resources.DeleteErrorUsedBy_f, Resources.AccountType, Resources.Account);
            if (Dao.Exists<AccountTransactionDocumentType>(x => x.MasterAccountTypeId == model.Id))
                return string.Format(Resources.DeleteErrorUsedBy_f, Resources.AccountType, Resources.DocumentType);
            return "";
        }
    }

    public class AccountDeleteValidator : SpecificationValidator<Account>
    {
        public override string GetErrorMessage(Account model)
        {
            if (Dao.Exists<Resource>(x => x.AccountId == model.Id))
                return string.Format(Resources.DeleteErrorUsedBy_f, Resources.Account, Resources.Resource);
            if (Dao.Exists<AccountTransactionValue>(x => x.AccountId == model.Id))
                return string.Format(Resources.DeleteErrorUsedBy_f, Resources.Account, Resources.AccountTransaction);
            return "";
        }
    }



}
