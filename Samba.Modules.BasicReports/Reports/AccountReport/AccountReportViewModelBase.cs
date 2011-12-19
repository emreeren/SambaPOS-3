using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Tickets;
using Samba.Domain.Models.Transactions;
using Samba.Localization.Properties;
using Samba.Persistance.Data;
using Samba.Services;

namespace Samba.Modules.BasicReports.Reports.AccountReport
{
    public abstract class AccountReportViewModelBase : ReportViewModelBase
    {
        protected AccountReportViewModelBase(IUserService userService, IApplicationState applicationState)
            : base(userService, applicationState)
        {

        }

        protected override void CreateFilterGroups()
        {
            FilterGroups.Clear();
            FilterGroups.Add(CreateWorkPeriodFilterGroup());
        }

        protected IEnumerable<AccountData> GetBalancedAccounts(bool selectInternalAccounts)
        {
            var tickets = Dao.Query<Ticket>(x => x.AccountId > 0, x => x.Payments);
            var paymentSum = tickets.GroupBy(x => x.AccountId).Select(x =>
                new
                {
                    AccountId = x.Key,
                    Amount = x.Sum(k => k.Payments.Where(y => y.PaymentType == 3).Sum(j => j.Amount))
                });

            var transactions = Dao.Query<CashTransaction>().Where(x => x.AccountId > 0);
            var transactionSum = transactions.GroupBy(x => x.AccountId).Select(
                x =>
                new
                {
                    AccountId = x.Key,
                    Amount = x.Sum(y => y.TransactionType == 1 ? y.Amount : 0 - y.Amount)
                });

            var accountTransactions = Dao.Query<AccountTransaction>().Where(x => x.AccountId > 0);
            var accountTransactionSum = accountTransactions.GroupBy(x => x.AccountId).Select(
                x =>
                new
                {
                    AccountId = x.Key,
                    Amount = x.Sum(y => y.TransactionType == 3 ? y.Amount : 0 - y.Amount)
                });


            var accountIds = paymentSum.Select(x => x.AccountId).Distinct();
            accountIds = accountIds.Union(transactionSum.Select(x => x.AccountId).Distinct());
            accountIds = accountIds.Union(accountTransactionSum.Select(x => x.AccountId).Distinct());

            var list = (from accountId in accountIds
                        let amount = transactionSum.Where(x => x.AccountId == accountId).Sum(x => x.Amount)
                        let account = accountTransactionSum.Where(x => x.AccountId == accountId).Sum(x => x.Amount)
                        let payment = paymentSum.Where(x => x.AccountId == accountId).Sum(x => x.Amount)
                        select new { AccountId = accountId, Amount = (amount + account + payment) })
                            .Where(x => x.Amount != 0).ToList();

            var cids = list.Select(x => x.AccountId).ToList();

            var accounts = Dao.Select<Account, AccountData>(
                    x => new AccountData { Id = x.Id, AccountName = x.Name, PhoneNumber = x.SearchString, Amount = 0 },
                    x => cids.Contains(x.Id));

            foreach (var accountData in accounts)
            {
                AccountData data = accountData;
                accountData.Amount = list.SingleOrDefault(x => x.AccountId == data.Id).Amount;
            }

            return accounts;
        }

        public FlowDocument CreateReport(string reportHeader, bool? returnReceivables, bool selectInternalAccounts)
        {
            var report = new SimpleReport("8cm");
            report.AddHeader("Samba POS");
            report.AddHeader(reportHeader);
            report.AddHeader(string.Format(Resources.As_f, DateTime.Now));

            var accounts = GetBalancedAccounts(selectInternalAccounts);
            if (returnReceivables != null)
                accounts = returnReceivables.GetValueOrDefault(false) ?
                                accounts.Where(x => x.Amount < 0) :
                                accounts.Where(x => x.Amount > 0);

            report.AddColumTextAlignment("Tablo", TextAlignment.Left, TextAlignment.Left, TextAlignment.Right);
            report.AddColumnLength("Tablo", "35*", "35*", "30*");


            if (accounts.Count() > 0)
            {
                report.AddTable("Tablo", Resources.Accounts, "", "");

                var total = 0m;
                foreach (var account in accounts)
                {
                    total += Math.Abs(account.Amount);
                    report.AddRow("Tablo", account.PhoneNumber, account.AccountName, Math.Abs(account.Amount).ToString(ReportContext.CurrencyFormat));
                }
                report.AddRow("Tablo", Resources.GrandTotal, "", total);
            }
            else
            {
                report.AddHeader(string.Format(Resources.NoTransactionsFoundFor_f, reportHeader));
            }

            return report.Document;
        }
    }
}
