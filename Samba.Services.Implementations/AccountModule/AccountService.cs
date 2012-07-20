using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text.RegularExpressions;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Resources;
using Samba.Infrastructure;
using Samba.Infrastructure.Data;
using Samba.Localization.Properties;
using Samba.Persistance.Data;
using Samba.Services.Common;

namespace Samba.Services.Implementations.AccountModule
{
    [Export(typeof(IAccountService))]
    public class AccountService : AbstractService, IAccountService
    {
        [ImportingConstructor]
        public AccountService()
        {
            ValidatorRegistry.RegisterDeleteValidator(new AccountDeleteValidator());
            ValidatorRegistry.RegisterDeleteValidator(new AccountTemplateDeleteValidator());
            ValidatorRegistry.RegisterDeleteValidator<AccountTransactionTemplate>(x => Dao.Exists<AccountTransactionDocumentTemplate>(y => y.TransactionTemplates.Any(z => z.Id == x.Id)), Resources.AccountTransactionTemplate, Resources.DocumentTemplate);
            ValidatorRegistry.RegisterSaveValidator(new NonDuplicateSaveValidator<Account>(string.Format(Resources.SaveErrorDuplicateItemName_f, Resources.Account)));
            ValidatorRegistry.RegisterSaveValidator(new NonDuplicateSaveValidator<AccountTemplate>(string.Format(Resources.SaveErrorDuplicateItemName_f, Resources.AccountTemplate)));
            ValidatorRegistry.RegisterSaveValidator(new NonDuplicateSaveValidator<AccountTransactionTemplate>(string.Format(Resources.SaveErrorDuplicateItemName_f, Resources.AccountTransactionTemplate)));
            ValidatorRegistry.RegisterSaveValidator(new NonDuplicateSaveValidator<AccountTransactionDocumentTemplate>(string.Format(Resources.SaveErrorDuplicateItemName_f, Resources.DocumentTemplate)));
        }

        private int? _accountCount;
        public int GetAccountCount()
        {
            return (int)(_accountCount ?? (_accountCount = Dao.Count<Resource>()));
        }

        public void CreateNewTransactionDocument(Account selectedAccount, AccountTransactionDocumentTemplate documentTemplate, string description, decimal amount)
        {
            using (var w = WorkspaceFactory.Create())
            {
                var document = documentTemplate.CreateDocument(selectedAccount, description, amount);
                w.Add(document);
                w.CommitChanges();
            }
        }

        public decimal GetAccountBalance(int accountId)
        {
            return Dao.Sum<AccountTransactionValue>(x => x.Debit - x.Credit, x => x.AccountId == accountId);
        }

        public string GetCustomData(Account account, string fieldName)
        {
            return "";
        }

        public string GetDescription(AccountTransactionDocumentTemplate documentTemplate, Account account)
        {
            var result = documentTemplate.DescriptionTemplate;
            if (string.IsNullOrEmpty(result)) return result;
            while (Regex.IsMatch(result, "\\[:([^\\]]+)\\]"))
            {
                var match = Regex.Match(result, "\\[:([^\\]]+)\\]");
                result = result.Replace(match.Groups[0].Value, GetCustomData(account, match.Groups[1].Value));
            }
            if (result.Contains("[MONTH]")) result = result.Replace("[MONTH]", DateTime.Now.ToMonthName());
            if (result.Contains("[NEXT MONTH]")) result = result.Replace("[NEXT MONTH]", DateTime.Now.ToNextMonthName());
            if (result.Contains("[WEEK]")) result = result.Replace("[WEEK]", DateTime.Now.WeekOfYear().ToString());
            if (result.Contains("[NEXT WEEK]")) result = result.Replace("[NEXT WEEK]", (DateTime.Now.NextWeekOfYear()).ToString());
            if (result.Contains("[YEAR]")) result = result.Replace("[YEAR]", (DateTime.Now.Year).ToString());
            if (result.Contains("[ACCOUNT NAME]")) result = result.Replace("[ACCOUNT NAME]", account.Name);
            return result;
        }

        public decimal GetDefaultAmount(AccountTransactionDocumentTemplate documentTemplate, Account account)
        {
            decimal result = 0;
            if (!string.IsNullOrEmpty(documentTemplate.DefaultAmount))
            {
                var da = documentTemplate.DefaultAmount;
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
            if (Dao.Exists<Account>(x => x.Id == accountId))
                return Dao.Select<Account, string>(x => x.Name, x => x.Id == accountId).First();
            return "";
        }

        public int GetAccountIdByName(string accountName)
        {
            var acName = accountName.ToLower();
            if (Dao.Exists<Account>(x => x.Name.ToLower() == acName))
                return Dao.Select<Account, int>(x => x.Id, x => x.Name.ToLower() == acName).FirstOrDefault();
            return 0;
        }

        public IEnumerable<Account> GetAccounts(params AccountTemplate[] accountTemplates)
        {
            if (accountTemplates.Count() == 0) return Dao.Query<Account>();
            var ids = accountTemplates.Select(x => x.Id);
            return Dao.Query<Account>(x => ids.Contains(x.AccountTemplateId));
        }

        public IEnumerable<string> GetCompletingAccountNames(int accountTemplateId, string accountName)
        {
            if (string.IsNullOrWhiteSpace(accountName)) return null;
            var lacn = accountName.ToLower();
            return Dao.Select<Account, string>(x => x.Name, x => x.AccountTemplateId == accountTemplateId && x.Name.ToLower().Contains(lacn));
        }

        public Account GetAccountById(int accountId)
        {
            return Dao.Single<Account>(x => x.Id == accountId);
        }

        public IEnumerable<AccountTemplate> GetAccountTemplates()
        {
            return Dao.Query<AccountTemplate>();
        }

        public override void Reset()
        {
            _accountCount = null;
        }
    }

    public class AccountTemplateDeleteValidator : SpecificationValidator<AccountTemplate>
    {
        public override string GetErrorMessage(AccountTemplate model)
        {
            if (Dao.Exists<Account>(x => x.AccountTemplateId == model.Id))
                return string.Format(Resources.DeleteErrorUsedBy_f, Resources.AccountTemplate, Resources.Account);
            if (Dao.Exists<AccountTransactionDocumentTemplate>(x => x.MasterAccountTemplateId == model.Id))
                return string.Format(Resources.DeleteErrorUsedBy_f, Resources.AccountTemplate, Resources.DocumentTemplate);
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
