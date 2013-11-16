using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Samba.Domain.Models.Accounts;
using Samba.Localization.Properties;
using Samba.Persistance.Data;
using Samba.Persistance.Specification;

namespace Samba.Services.Common
{
    public class AccountTransactionSummaryBuilder
    {
        private DateTime? _start;
        private DateTime? _end;
        private Account _account;

        public static AccountTransactionSummaryBuilder Create()
        {
            return new AccountTransactionSummaryBuilder();
        }

        public AccountTransactionSummaryBuilder WithStartDate(DateTime? start)
        {
            if (start.HasValue)
            {
                _start = start;
                return WithEndDate(start);
            }
            return this;
        }

        public AccountTransactionSummaryBuilder WithEndDate(DateTime? end)
        {
            if (end.HasValue)
                _end = end.GetValueOrDefault();
            return this;
        }

        public AccountTransactionSummaryBuilder ForAccount(Account account)
        {
            _account = account;
            return this;
        }

        public AccountTransactionSummary Build()
        {
            var result = new AccountTransactionSummary();
            result.Update(_account, _start, _end);
            return result;
        }
    }

    public class AccountTransactionSummary
    {
        public AccountTransactionSummary()
        {
            Summaries = new List<AccountSummaryData>();
        }

        public IList<AccountDetailData> Transactions { get; set; }
        public IList<AccountSummaryData> Summaries { get; set; }
        public DateTime? Start { get; set; }
        public DateTime? End { get; set; }

        private Expression<Func<AccountTransactionValue, bool>> GetCurrentRange(DateTime? start, DateTime? end, Expression<Func<AccountTransactionValue, bool>> activeSpecification)
        {
            var result = activeSpecification;
            if (start.HasValue)
            {
                var currentStart = start.GetValueOrDefault();
                result = result.And(x => x.Date >= currentStart);
                if (end.HasValue)
                {
                    var currentEnd = end.GetValueOrDefault();
                    if (end != start)
                    {
                        result = result.And(x => x.Date <= currentEnd);
                    }
                }
            }
            return result;
        }

        private Expression<Func<AccountTransactionValue, bool>> GetPastRange(DateTime? start, Expression<Func<AccountTransactionValue, bool>> activeSpecification)
        {
            var result = activeSpecification;
            if (start.HasValue)
            {
                var currentStart = start.GetValueOrDefault();
                result = result.And(x => x.Date < currentStart);
            }
            return result;
        }

        private Expression<Func<AccountTransactionValue, bool>> GetFutureRange(DateTime? end, Expression<Func<AccountTransactionValue, bool>> activeSpecification)
        {
            var result = activeSpecification;
            if (end.HasValue)
            {
                var currentEnd = end.GetValueOrDefault();
                result = result.And(x => x.Date > currentEnd);
            }
            return result;
        }

        public void Update(Account selectedAccount, DateTime? start, DateTime? end)
        {
            Start = start;
            End = end;
            var transactions = Dao.Query(GetCurrentRange(start, end, x => x.AccountId == selectedAccount.Id)).OrderBy(x => x.Date);
            Transactions = transactions.Select(x => new AccountDetailData(x, selectedAccount)).ToList();
            if (start.HasValue)
            {
                var pastDebit = GetPastDebit(selectedAccount, start);
                var pastCredit = GetPastCredit(selectedAccount, start);
                var pastExchange = GetPastExchange(selectedAccount, start);
                if (pastCredit > 0 || pastDebit > 0)
                {
                    Summaries.Add(new AccountSummaryData(Resources.TransactionTotal, Transactions.Sum(x => x.Debit), Transactions.Sum(x => x.Credit)));
                    var detailValue =
                        new AccountDetailData(
                        new AccountTransactionValue
                        {
                            Date = start.GetValueOrDefault(),
                            Name = Resources.BalanceBroughtForward,
                            Credit = pastCredit,
                            Debit = pastDebit,
                            Exchange = pastExchange
                        }, selectedAccount) { IsBold = true };
                    Transactions.Insert(0, detailValue);
                }
            }
            if (end.HasValue && end != start)
            {
                var futureDebit = GetFutureDebit(selectedAccount, end);
                var futureCredit = GetFutureCredit(selectedAccount, end);
                var futureExchange = GetFutureExchange(selectedAccount, end);
                if (futureCredit > 0 || futureDebit > 0)
                {
                    Summaries.Add(new AccountSummaryData(Resources.DateRangeTotal, Transactions.Sum(x => x.Debit), Transactions.Sum(x => x.Credit)));
                    var detailValue =
                        new AccountDetailData(
                        new AccountTransactionValue
                        {
                            Date = end.GetValueOrDefault(),
                            Name = Resources.BalanceAfterDate,
                            Credit = futureCredit,
                            Debit = futureDebit,
                            Exchange = futureExchange
                        }, selectedAccount) { IsBold = true };
                    Transactions.Add(detailValue);
                }
            }

            Summaries.Add(new AccountSummaryData(Resources.GrandTotal, Transactions.Sum(x => x.Debit), Transactions.Sum(x => x.Credit)));

            for (var i = 0; i < Transactions.Count; i++)
            {
                Transactions[i].Balance = (Transactions[i].Debit - Transactions[i].Credit);
                if (i > 0) (Transactions[i].Balance) += (Transactions[i - 1].Balance);
            }
        }

        private decimal GetFutureExchange(Account selectedAccount, DateTime? end)
        {
            if (selectedAccount.ForeignCurrencyId > 0) return 0;
            return Dao.Sum(x => x.Exchange, GetFutureRange(end, x => x.AccountId == selectedAccount.Id));
        }

        private decimal GetFutureCredit(Account selectedAccount, DateTime? end)
        {
            if (selectedAccount.ForeignCurrencyId > 0)
                return Math.Abs(Dao.Sum(x => x.Exchange, GetFutureRange(end, x => x.AccountId == selectedAccount.Id && x.Debit == 0)));
            return Dao.Sum(x => x.Credit, GetFutureRange(end, x => x.AccountId == selectedAccount.Id));
        }

        private decimal GetFutureDebit(Account selectedAccount, DateTime? end)
        {
            if (selectedAccount.ForeignCurrencyId > 0)
                return Math.Abs(Dao.Sum(x => x.Exchange, GetFutureRange(end, x => x.AccountId == selectedAccount.Id && x.Credit == 0)));
            return Dao.Sum(x => x.Debit, GetFutureRange(end, x => x.AccountId == selectedAccount.Id));
        }

        private decimal GetPastExchange(Account selectedAccount, DateTime? start)
        {
            if (selectedAccount.ForeignCurrencyId > 0) return 0;
            return Dao.Sum(x => x.Exchange, GetPastRange(start, x => x.AccountId == selectedAccount.Id));
        }

        private decimal GetPastCredit(Account selectedAccount, DateTime? start)
        {
            if (selectedAccount.ForeignCurrencyId > 0)
                return Math.Abs(Dao.Sum(x => x.Exchange, GetPastRange(start, x => x.AccountId == selectedAccount.Id && x.Debit == 0)));
            return Dao.Sum(x => x.Credit, GetPastRange(start, x => x.AccountId == selectedAccount.Id));
        }

        private decimal GetPastDebit(Account selectedAccount, DateTime? start)
        {
            if (selectedAccount.ForeignCurrencyId > 0)
                return Math.Abs(Dao.Sum(x => x.Exchange, GetPastRange(start, x => x.AccountId == selectedAccount.Id && x.Credit == 0)));
            return Dao.Sum(x => x.Debit, GetPastRange(start, x => x.AccountId == selectedAccount.Id));
        }
    }
}