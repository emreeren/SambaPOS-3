using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Settings;
using Samba.Infrastructure.Settings;
using Samba.Localization.Properties;
using Samba.Services.Common;

namespace Samba.Services.Implementations
{
    [Export(typeof(IReportService))]
    class ReportService : IReportService
    {
        private readonly IAccountService _accountService;
        private readonly IPrinterService _printerService;
        private readonly ISettingService _settingService;

        [ImportingConstructor]
        public ReportService(IAccountService accountService, IPrinterService printerService, ISettingService settingService)
        {
            _accountService = accountService;
            _printerService = printerService;
            _settingService = settingService;
        }

        public void PrintAccountScreen(AccountScreen accountScreen, WorkPeriod workperiod, Printer printer)
        {
            var accounts = _accountService.GetAccountScreenRows(accountScreen, workperiod);
            var report = new SimpleReport("");
            report.AddParagraph("Header");
            report.AddParagraphLine("Header", _settingService.ProgramSettings.UserInfo);
            report.AddParagraphLine("Header", string.Format(accountScreen.Name), true);
            report.AddParagraphLine("Header", "");

            report.AddColumnLength("Transactions", "60*", "40*");
            report.AddColumTextAlignment("Transactions", TextAlignment.Left, TextAlignment.Right);
            report.AddTable("Transactions", string.Format(Resources.Name_f, Resources.Account), Resources.Balance);

            foreach (var ad in accounts)
            {
                report.AddRow("Transactions", ad.Name, ad.BalanceStr);
            }

            _printerService.PrintReport(report.Document, printer);
        }

        public void PrintAccountTransactions(Account account, WorkPeriod workPeriod, Printer printer, string filter)
        {
            var range = _accountService.GetDateRange(filter, workPeriod);
            var summary = _accountService.GetAccountTransactionSummary(account, workPeriod, range.Start, range.End);

            var totalBalance = summary.Transactions.Sum(x => x.Debit - x.Credit).ToString(LocalSettings.ReportCurrencyFormat);

            var report = new SimpleReport("");
            report.AddParagraph("Header");
            report.AddParagraphLine("Header", _settingService.ProgramSettings.UserInfo);
            report.AddParagraphLine("Header", Resources.AccountTransaction, true);
            report.AddParagraphLine("Header", "");
            report.AddParagraphLine("Header", string.Format("{0}: {1}", string.Format(Resources.Name_f, Resources.Account), account.Name));
            report.AddParagraphLine("Header", string.Format("{0}: {1}", Resources.Balance, totalBalance));
            report.AddParagraphLine("Header", "");

            report.AddColumnLength("Transactions", "15*", "35*", "15*", "15*", "20*");
            report.AddColumTextAlignment("Transactions", TextAlignment.Left, TextAlignment.Left, TextAlignment.Right, TextAlignment.Right, TextAlignment.Right);
            report.AddTable("Transactions", Resources.Date, Resources.Description, Resources.Debit, Resources.Credit, Resources.Balance);

            foreach (var ad in summary.Transactions)
            {
                report.AddRow("Transactions", ad.Date.ToShortDateString(), ad.Name, ad.DebitStr, ad.CreditStr, ad.BalanceStr);
            }

            foreach (var sum in summary.Summaries)
            {
                report.AddBoldRow("Transactions", "", sum.Caption, sum.Debit, sum.Credit, sum.Balance);
            }

            _printerService.PrintReport(report.Document, printer);
        }
    }
}