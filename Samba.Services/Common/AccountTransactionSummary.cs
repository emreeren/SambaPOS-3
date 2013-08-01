using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Settings;
using Samba.Localization;
using Samba.Localization.Properties;
using Samba.Persistance.Data;
using Samba.Persistance.Specification;

namespace Samba.Services.Common
{
    public class AccountTransactionSummary
    {
        public AccountTransactionSummary(ICacheService cacheService, Account selectedAccount, WorkPeriod currentWorkPeriod)
        {
            Summaries = new List<AccountSummaryData>();
            Update(cacheService, selectedAccount, currentWorkPeriod);
        }

        public IList<AccountDetailData> Transactions { get; set; }
        public IList<AccountSummaryData> Summaries { get; set; }

        private Expression<Func<AccountTransactionValue, bool>> GetCurrentRange(int filterType, Expression<Func<AccountTransactionValue, bool>> activeSpecification, WorkPeriod workPeriod)
        {
            //Resources.All, Resources.Month, Resources.Week, Resources.WorkPeriod
            if (filterType == 1) return activeSpecification.And(x => x.Date >= DateTime.Now.MonthStart());
            if (filterType == 2) return activeSpecification.And(x => x.Date >= DateTime.Now.StartOfWeek());
            if (filterType == 3) return activeSpecification.And(x => x.Date >= workPeriod.StartDate);
            return activeSpecification;
        }

        private Expression<Func<AccountTransactionValue, bool>> GetPastRange(int filterType, Expression<Func<AccountTransactionValue, bool>> activeSpecification, WorkPeriod workPeriod)
        {
            //Resources.All, Resources.Month, Resources.Week, Resources.WorkPeriod
            if (filterType == 1) return activeSpecification.And(x => x.Date < DateTime.Now.MonthStart());
            if (filterType == 2) return activeSpecification.And(x => x.Date < DateTime.Now.StartOfWeek());
            if (filterType == 3) return activeSpecification.And(x => x.Date < workPeriod.StartDate);
            return activeSpecification;
        }

        public void Update(ICacheService cacheService, Account selectedAccount, WorkPeriod currentWorkPeriod)
        {
            var accountType = cacheService.GetAccountTypeById(selectedAccount.AccountTypeId);
            var transactions = Dao.Query(GetCurrentRange(accountType.DefaultFilterType, x => x.AccountId == selectedAccount.Id, currentWorkPeriod)).OrderBy(x => x.Date);
            Transactions = transactions.Select(x => new AccountDetailData(x, selectedAccount)).ToList();
            if (accountType.DefaultFilterType > 0)
            {
                var pastDebit = Dao.Sum(x => x.Debit, GetPastRange(accountType.DefaultFilterType, x => x.AccountId == selectedAccount.Id, currentWorkPeriod));
                var pastCredit = Dao.Sum(x => x.Credit, GetPastRange(accountType.DefaultFilterType, x => x.AccountId == selectedAccount.Id, currentWorkPeriod));
                var pastExchange = Dao.Sum(x => x.Exchange, GetPastRange(accountType.DefaultFilterType, x => x.AccountId == selectedAccount.Id, currentWorkPeriod));
                if (pastCredit > 0 || pastDebit > 0)
                {
                    Summaries.Add(new AccountSummaryData(Resources.Total, Transactions.Sum(x => x.Debit), Transactions.Sum(x => x.Credit)));
                    var detailValue =
                        new AccountDetailData(
                        new AccountTransactionValue
                        {
                            Name = Resources.PastTransactions,
                            Credit = pastCredit,
                            Debit = pastDebit,
                            Exchange = pastExchange
                        }, selectedAccount) { IsBold = true };
                    Transactions.Insert(0, detailValue);
                }
            }

            Summaries.Add(new AccountSummaryData(Resources.GrandTotal, Transactions.Sum(x => x.Debit), Transactions.Sum(x => x.Credit)));

            for (var i = 0; i < Transactions.Count; i++)
            {
                Transactions[i].Balance = (Transactions[i].Debit - Transactions[i].Credit);
                if (i > 0) (Transactions[i].Balance) += (Transactions[i - 1].Balance);
            }
        }
    }
}