using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Linq.Expressions;
using Samba.Domain.Models;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Entities;
using Samba.Domain.Models.Settings;
using Samba.Domain.Models.Tickets;
using Samba.Infrastructure.Data;
using Samba.Infrastructure.Data.Validation;
using Samba.Localization.Properties;
using Samba.Persistance.Common;
using Samba.Persistance.Data;
using Samba.Persistance.Specification;

namespace Samba.Persistance.Implementations
{
    [Export(typeof(IAccountDao))]
    class AccountDao : IAccountDao
    {
        [ImportingConstructor]
        public AccountDao()
        {
            ValidatorRegistry.RegisterDeleteValidator(new AccountDeleteValidator());
            ValidatorRegistry.RegisterDeleteValidator(new AccountTypeDeleteValidator());
            ValidatorRegistry.RegisterDeleteValidator(new AccountTransactionTypeDeleteValidator());
            ValidatorRegistry.RegisterDeleteValidator(new AccountTransactionDocumentTypeDeleteValidator());
            ValidatorRegistry.RegisterSaveValidator(new NonDuplicateSaveValidator<Account>(string.Format(Resources.SaveErrorDuplicateItemName_f, Resources.Account)));
            ValidatorRegistry.RegisterSaveValidator(new NonDuplicateSaveValidator<AccountType>(string.Format(Resources.SaveErrorDuplicateItemName_f, Resources.AccountType)));
            ValidatorRegistry.RegisterSaveValidator(new NonDuplicateSaveValidator<AccountTransactionType>(string.Format(Resources.SaveErrorDuplicateItemName_f, Resources.AccountTransactionType)));
            ValidatorRegistry.RegisterSaveValidator(new NonDuplicateSaveValidator<AccountTransactionDocumentType>(string.Format(Resources.SaveErrorDuplicateItemName_f, Resources.DocumentType)));
        }

        public decimal GetAccountBalance(int accountId)
        {
            return Dao.Sum<AccountTransactionValue>(x => x.Debit - x.Credit, x => x.AccountId == accountId);
        }

        public decimal GetAccountExchangeBalance(int accountId)
        {
            return Dao.Sum<AccountTransactionValue>(x => x.Exchange, x => x.AccountId == accountId);
        }

        public IEnumerable<Account> GetAccountsByTypeId(int accountTypeId)
        {
            return Dao.Query<Account>(x => x.AccountTypeId == accountTypeId);
        }

        public IEnumerable<Account> GetAccountsByIds(IEnumerable<int> accountIds)
        {
            var ids = accountIds.ToList();
            return ids.Any() ? Dao.Query<Account>(x => ids.Contains(x.Id)) : new List<Account>();
        }

        public IEnumerable<Account> GetBalancedAccountsByAccountTypeId(int accountTypeId)
        {
            using (var w = WorkspaceFactory.CreateReadOnly())
            {
                var q1 = w.Queryable<AccountTransactionValue>().GroupBy(x => x.AccountId).Where(
                        x => x.Sum(y => y.Debit - y.Credit) != 0).Select(x => x.Key);
                return w.Queryable<Account>().Where(x => x.AccountTypeId == accountTypeId && q1.Contains(x.Id)).ToList();
            }
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

        public string GetEntityCustomDataByAccountId(int accountId)
        {
            return Dao.Select<Entity, string>(x => x.CustomData, x => x.AccountId == accountId).SingleOrDefault();
        }

        public AccountTransactionDocument CreateTransactionDocument(Account selectedAccount, AccountTransactionDocumentType documentType, string description, decimal amount, decimal exchangeRate, IEnumerable<AccountData> accounts, IEnumerable<ForeignCurrency> currencies)
        {
            using (var w = WorkspaceFactory.Create())
            {
                var document = documentType.CreateDocument(selectedAccount, description, amount, exchangeRate, accounts != null ? accounts.ToList() : null, currencies.ToList());
                w.Add(document);
                w.CommitChanges();
                return document;
            }
        }

        public Account GetAccountById(int accountId)
        {
            return Dao.Single<Account>(x => x.Id == accountId);
        }

        public Account GetAccountByName(string accountName)
        {
            return Dao.Single<Account>(x => x.Name == accountName);
        }

        public bool GetIsAccountNameExists(string accountName)
        {
            return Dao.Exists<Account>(x => x.Name == accountName);
        }

        public int CreateAccount(int accountTypeId, string accountName)
        {
            using (var w = WorkspaceFactory.Create())
            {
                var account = new Account { AccountTypeId = accountTypeId, Name = accountName };
                w.Add(account);
                w.CommitChanges();
                return account.Id;
            }
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

        public IEnumerable<string> GetAccountNames(Expression<Func<Account, bool>> predictate)
        {
            return Dao.Select(x => x.Name, predictate);
        }

        public AccountTransactionDocument GetAccountTransactionDocumentById(int documentId)
        {
            return Dao.Single<AccountTransactionDocument>(x => x.Id == documentId, x => x.AccountTransactions.Select(y => y.AccountTransactionValues));
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

    public class AccountTransactionDocumentTypeDeleteValidator : SpecificationValidator<AccountTransactionDocumentType>
    {
        public override string GetErrorMessage(AccountTransactionDocumentType model)
        {
            if (Dao.Exists<AccountTransactionDocument>(x => x.DocumentTypeId == model.Id))
                return string.Format(Resources.DeleteErrorUsedBy_f, Resources.DocumentType, Resources.AccountTransactionDocument);
            return "";
        }
    }

    public class AccountTransactionTypeDeleteValidator : SpecificationValidator<AccountTransactionType>
    {
        public override string GetErrorMessage(AccountTransactionType model)
        {
            if (Dao.Exists<AccountTransactionDocumentType>(x => x.TransactionTypes.Any(y => y.Id == model.Id)))
            {
                return string.Format(Resources.DeleteErrorUsedBy_f, Resources.AccountTransactionType,
                                     Resources.AccountTransactionDocument);
            }

            if (Dao.Exists<PaymentType>(x => x.AccountTransactionType.Id == model.Id))
            {
                return string.Format(Resources.DeleteErrorUsedBy_f, Resources.AccountTransactionType,
                                     Resources.PaymentType);
            }

            if (Dao.Exists<CalculationType>(x => x.AccountTransactionType.Id == model.Id))
            {
                return string.Format(Resources.DeleteErrorUsedBy_f, Resources.AccountTransactionType,
                                     Resources.CalculationType);
            }

            if (Dao.Exists<TicketType>(x => x.SaleTransactionType.Id == model.Id))
            {
                return string.Format(Resources.DeleteErrorUsedBy_f, Resources.AccountTransactionType,
                                     Resources.TicketType);
            }
            return "";
        }
    }

    public class AccountDeleteValidator : SpecificationValidator<Account>
    {
        public override string GetErrorMessage(Account model)
        {
            if (Dao.Exists<PaymentType>(x => x.Account.Id == model.Id))
                return string.Format(Resources.DeleteErrorUsedBy_f, Resources.Account, Resources.PaymentType);
            if (Dao.Exists<Entity>(x => x.AccountId == model.Id))
                return string.Format(Resources.DeleteErrorUsedBy_f, Resources.Account, Resources.Entity);
            if (Dao.Exists<AccountTransactionValue>(x => x.AccountId == model.Id))
                return string.Format(Resources.DeleteErrorUsedBy_f, Resources.Account, Resources.AccountTransaction);
            return "";
        }
    }
}
