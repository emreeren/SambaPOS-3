using System.ComponentModel.Composition;
using Samba.Domain.Models.Accounts;
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
            ValidatorRegistry.RegisterDeleteValidator(new AccountTemplateDeleteValidator());
        }

        private int? _accountCount;
        public int GetAccountCount()
        {
            return (int)(_accountCount ?? (_accountCount = Dao.Count<Account>()));
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

        public decimal GetAccountBalance(Account account)
        {
            return Dao.Sum<AccountTransactionValue>(x => x.Debit - x.Credit, x => x.AccountId == account.Id);
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
                return Resources.DeleteErrorAccountTemplateAssignedtoAccounts;
            return "";
        }
    }
}
