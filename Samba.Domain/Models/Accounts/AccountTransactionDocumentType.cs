using System.Collections.Generic;
using System.Linq;
using Samba.Domain.Models.Settings;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Accounts
{
    public class AccountTransactionDocumentType : EntityClass, IOrderable
    {
        public AccountTransactionDocumentType()
        {
            _transactionTypes = new List<AccountTransactionType>();
            _accountTransactionDocumentTypeMaps = new List<AccountTransactionDocumentTypeMap>();
            _accountTransactionDocumentAccountMaps = new List<AccountTransactionDocumentAccountMap>();
        }

        public string ButtonHeader { get; set; }

        private string _buttonColor;
        public string ButtonColor
        {
            get { return _buttonColor ?? "Gainsboro"; }
            set { _buttonColor = value; }
        }

        public int MasterAccountTypeId { get; set; }

        private IList<AccountTransactionType> _transactionTypes;
        public virtual IList<AccountTransactionType> TransactionTypes
        {
            get { return _transactionTypes; }
            set { _transactionTypes = value; }
        }

        private IList<AccountTransactionDocumentTypeMap> _accountTransactionDocumentTypeMaps;
        public virtual IList<AccountTransactionDocumentTypeMap> AccountTransactionDocumentTypeMaps
        {
            get { return _accountTransactionDocumentTypeMaps; }
            set { _accountTransactionDocumentTypeMaps = value; }
        }

        private IList<AccountTransactionDocumentAccountMap> _accountTransactionDocumentAccountMaps;
        public virtual IList<AccountTransactionDocumentAccountMap> AccountTransactionDocumentAccountMaps
        {
            get { return _accountTransactionDocumentAccountMaps; }
            set { _accountTransactionDocumentAccountMaps = value; }
        }

        public string DefaultAmount { get; set; }
        public string DescriptionTemplate { get; set; }
        public string ExchangeTemplate { get; set; }
        public bool BatchCreateDocuments { get; set; }
        public int Filter { get; set; }
        public int SortOrder { get; set; }
        public int PrinterTemplateId { get; set; }

        public string UserString
        {
            get { return Name; }
        }

        public AccountTransactionDocument CreateDocument(Account account, string description, decimal amount, decimal exchangeRate, IList<AccountData> accounts, IList<ForeignCurrency> currencies)
        {
            var result = new AccountTransactionDocument { Name = Name, DocumentTypeId = Id };
            foreach (var accountTransactionType in TransactionTypes)
            {
                var transaction = AccountTransaction.Create(accountTransactionType);
                var amountRate = GetExchangeRate(accountTransactionType.ForeignCurrencyId, currencies);
                amount = amount * amountRate;
                transaction.UpdateAmount(amount, exchangeRate, accounts);
                transaction.UpdateAccount(MasterAccountTypeId, account.Id);
                if (accounts != null && accounts.Count > 0)
                {
                    if (transaction.SourceAccountTypeId != MasterAccountTypeId &&
                        transaction.SourceTransactionValue.AccountId == 0)
                    {
                        var ac = accounts.FirstOrDefault(x => x.AccountTypeId == transaction.SourceAccountTypeId);
                        if (ac != null) transaction.SetSourceAccount(ac.AccountTypeId, ac.AccountId);
                    }

                    if (transaction.TargetAccountTypeId != MasterAccountTypeId &&
                        transaction.TargetTransactionValue.AccountId == 0)
                    {
                        var ac = accounts.FirstOrDefault(x => x.AccountTypeId == transaction.TargetAccountTypeId);
                        if (ac != null) transaction.SetTargetAccount(ac.AccountTypeId, ac.AccountId);
                    }
                }
                if (!string.IsNullOrEmpty(description))
                {
                    transaction.UpdateDescription(description);
                }
                result.AccountTransactions.Add(transaction);
            }
            return result;
        }

        private decimal GetExchangeRate(int foreignCurrencyId, IEnumerable<ForeignCurrency> currencies)
        {
            if (foreignCurrencyId == 0) return 1;
            var fc = currencies.FirstOrDefault(x => x.Id == foreignCurrencyId);
            return fc != null ? fc.ExchangeRate : 1;
        }

        public void AddAccountTransactionDocumentTypeMap()
        {
            AccountTransactionDocumentTypeMaps.Add(new AccountTransactionDocumentTypeMap());
        }

        public List<int> GetNeededAccountTypes()
        {
            var result = new List<int>();
            foreach (var accountTransactionType in TransactionTypes)
            {
                if (accountTransactionType.TargetAccountTypeId != MasterAccountTypeId &&
                    accountTransactionType.DefaultTargetAccountId == 0)
                {
                    if (!result.Contains(accountTransactionType.TargetAccountTypeId))
                        result.Add(accountTransactionType.TargetAccountTypeId);
                }
                if (accountTransactionType.SourceAccountTypeId != MasterAccountTypeId &&
                    accountTransactionType.DefaultSourceAccountId == 0)
                {
                    if (!result.Contains(accountTransactionType.SourceAccountTypeId))
                        result.Add(accountTransactionType.SourceAccountTypeId);
                }
            }
            return result;
        }

        public bool CanMakeAccountTransaction(Account selectedAccount)
        {
            return TransactionTypes.Any(x => x.CanMakeAccountTransaction(selectedAccount));
        }
    }
}
