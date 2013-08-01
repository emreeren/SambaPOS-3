using System.Collections.Generic;
using Samba.Domain.Models.Accounts;
using Samba.Infrastructure.Settings;
using Samba.Persistance.Common;

namespace Samba.Services.Common
{
    public class AccountScreenRow
    {
        public AccountScreenRow(string name, decimal balance, decimal exchange, int accountId, string currencyFormat, int accountTypeId, string groupKey)
        {
            Name = name;
            Balance = balance;
            if (!string.IsNullOrEmpty(currencyFormat)) Exchange = exchange;
            CurrencyFormat = currencyFormat;
            AccountId = accountId;
            AccountTypeId = accountTypeId;
            GroupKey = groupKey;
        }

        protected string CurrencyFormat { get; set; }
        public int AccountId { get; set; }
        public string BalanceStr
        {
            get
            {
                return !string.IsNullOrEmpty(ExchangeStr)
                           ? ExchangeStr
                           : Balance.ToString(LocalSettings.ReportCurrencyFormat);
            }
        }
        public string ExchangeStr { get { return Exchange != Balance ? string.Format(CurrencyFormat, Exchange) : ""; } }
        public string Name { get; set; }
        public decimal Balance { get; set; }
        public decimal Exchange { get; set; }
        public string Fill { get; set; }
        public int AccountTypeId { get; set; }
        public string GroupKey { get; set; }

        public static AccountScreenRow Create(KeyValuePair<Account, BalanceValue> kvp, string currencyFormat, string groupKey)
        {
            return new AccountScreenRow(kvp.Key.Name, kvp.Value.Balance, kvp.Value.Exchange, kvp.Key.Id, currencyFormat, kvp.Key.AccountTypeId, groupKey);
        }

        public static AccountScreenRow Create(KeyValuePair<AccountType, BalanceValue> kvp, string groupKey)
        {
            return new AccountScreenRow(kvp.Key.Name, kvp.Value.Balance, kvp.Value.Exchange, 0, "", kvp.Key.Id, groupKey);
        }
    }
}