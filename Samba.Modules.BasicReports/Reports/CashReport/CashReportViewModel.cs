using System.Linq;
using System.Windows;
using System.Windows.Documents;
using Samba.Domain;
using Samba.Localization.Properties;
using Samba.Services;

namespace Samba.Modules.BasicReports.Reports.CashReport
{
    public class CashReportViewModel : ReportViewModelBase
    {
        public CashReportViewModel(IUserService userService, IApplicationState applicationState)
            : base(userService, applicationState)
        {
        }

        protected override void CreateFilterGroups()
        {
            FilterGroups.Clear();
            FilterGroups.Add(CreateWorkPeriodFilterGroup());
        }

        private static string GetPaymentString(int paymentType)
        {
            if (paymentType == (int)PaymentType.Cash) return Resources.Cash;
            if (paymentType == (int)PaymentType.CreditCard) return Resources.CreditCard_ab;
            return Resources.Voucher_ab;
        }

        private static string Fs(decimal amount)
        {
            return amount.ToString(ReportContext.CurrencyFormat);
        }

        protected override FlowDocument GetReport()
        {
            var report = new SimpleReport("8cm");
            AddDefaultReportHeader(report, ReportContext.CurrentWorkPeriod, Resources.CashReport);

            if (ReportContext.CurrentWorkPeriod.Id == 0)
            {
                report.AddHeader(" ");
                report.AddHeader(Resources.DateRangeIsNotActiveWorkPeriod);
                report.AddHeader(Resources.ReportDoesNotContainsCashState);
            }

            var cashExpenseTotal = ReportContext.CashTransactions
                .Where(x => x.PaymentType == (int)PaymentType.Cash && x.TransactionType == (int)TransactionType.Expense)
                .Sum(x => x.Amount);
            var creditCardExpenseTotal = ReportContext.CashTransactions
                .Where(x => x.PaymentType == (int)PaymentType.CreditCard && x.TransactionType == (int)TransactionType.Expense)
                .Sum(x => x.Amount);
            var ticketExpenseTotal = ReportContext.CashTransactions
               .Where(x => x.PaymentType == (int)PaymentType.Ticket && x.TransactionType == (int)TransactionType.Expense)
               .Sum(x => x.Amount);

            var cashIncomeTotal = ReportContext.CashTransactions
                .Where(x => x.PaymentType == (int)PaymentType.Cash && x.TransactionType == (int)TransactionType.Income)
                .Sum(x => x.Amount);
            var ticketIncomeTotal = ReportContext.CashTransactions
                .Where(x => x.PaymentType == (int)PaymentType.Ticket && x.TransactionType == (int)TransactionType.Income)
                .Sum(x => x.Amount);
            var creditCardIncomeTotal = ReportContext.CashTransactions
                .Where(x => x.PaymentType == (int)PaymentType.CreditCard && x.TransactionType == (int)TransactionType.Income)
                .Sum(x => x.Amount);


            var expenseTransactions =
                 ReportContext.CashTransactions.Where(x => x.TransactionType == (int)TransactionType.Expense);

            if (expenseTransactions.Count() > 0)
            {
                report.AddColumTextAlignment("Gider", TextAlignment.Left, TextAlignment.Left, TextAlignment.Right);
                report.AddColumnLength("Gider", "15*", "Auto", "25*");
                report.AddTable("Gider", Resources.Expenses, "", "");

                report.AddBoldRow("Gider", Resources.CashTransactions.ToUpper(), "", "");
                foreach (var cashTransaction in expenseTransactions)
                {
                    report.AddRow("Gider", GetPaymentString(cashTransaction.PaymentType),
                        Fct(cashTransaction), Fs(cashTransaction.Amount));
                }

                report.AddBoldRow("Gider", Resources.Totals.ToUpper(), "", "");
                report.AddRow("Gider", GetPaymentString(0), Resources.TotalExpense, Fs(cashExpenseTotal));
                report.AddRow("Gider", GetPaymentString(1), Resources.TotalExpense, Fs(creditCardExpenseTotal));
                report.AddRow("Gider", GetPaymentString(2), Resources.TotalExpense, Fs(ticketExpenseTotal));
                report.AddRow("Gider", Resources.GrandTotal.ToUpper(), "", Fs(cashExpenseTotal + creditCardExpenseTotal + ticketExpenseTotal));

            }


            var ac = ReportContext.GetOperationalAmountCalculator();

            report.AddColumTextAlignment("Gelir", TextAlignment.Left, TextAlignment.Left, TextAlignment.Right);
            report.AddColumnLength("Gelir", "15*", "Auto", "25*");
            report.AddTable("Gelir", Resources.Incomes, "", "");

            if (ReportContext.CurrentWorkPeriod.Id > 0) //devreden rakamları aktif çalışma dönemlerinden biri seçildiyse çalışır
            {
                var total = ReportContext.CurrentWorkPeriod.CashAmount
                            + ReportContext.CurrentWorkPeriod.CreditCardAmount
                            + ReportContext.CurrentWorkPeriod.TicketAmount;
                if (total > 0)
                {
                    report.AddBoldRow("Gelir", Resources.StartAmount.ToUpper(), "", "");
                    if (ReportContext.CurrentWorkPeriod.CashAmount > 0)
                        report.AddRow("Gelir", GetPaymentString(0) + " " + Resources.StartAmount, "", Fs(ReportContext.CurrentWorkPeriod.CashAmount));
                    if (ReportContext.CurrentWorkPeriod.CreditCardAmount > 0)
                        report.AddRow("Gelir", GetPaymentString(1) + " " + Resources.StartAmount, "", Fs(ReportContext.CurrentWorkPeriod.CreditCardAmount));
                    if (ReportContext.CurrentWorkPeriod.TicketAmount > 0)
                        report.AddRow("Gelir", GetPaymentString(2) + " " + Resources.StartAmount, "", Fs(ReportContext.CurrentWorkPeriod.TicketAmount));
                    report.AddRow("Gelir", Resources.Total.ToUpper(), "", Fs(total));
                }
            }


            var incomeTransactions =
                ReportContext.CashTransactions.Where(x => x.TransactionType == (int)TransactionType.Income);

            if (incomeTransactions.Count() > 0)
            {
                report.AddBoldRow("Gelir", Resources.SalesIncome.ToUpper(), "", "");
                if (ac.CashTotal > 0)
                    report.AddRow("Gelir", GetPaymentString(0) + " " + Resources.SalesIncome, "", Fs(ac.CashTotal));
                if (ac.CreditCardTotal > 0)
                    report.AddRow("Gelir", GetPaymentString(1) + " " + Resources.SalesIncome, "", Fs(ac.CreditCardTotal));
                if (ac.TicketTotal > 0)
                    report.AddRow("Gelir", GetPaymentString(2) + " " + Resources.SalesIncome, "", Fs(ac.TicketTotal));

                report.AddRow("Gelir", Resources.Total.ToUpper(), "", Fs(ac.CashTotal
                                                               + ac.CreditCardTotal
                                                               + ac.TicketTotal));


                report.AddBoldRow("Gelir", Resources.CashTransactions.ToUpper(), "", "");
                var it = 0m;
                foreach (var cashTransaction in incomeTransactions)
                {
                    it += cashTransaction.Amount;
                    report.AddRow("Gelir", GetPaymentString(cashTransaction.PaymentType),
                        Fct(cashTransaction),
                        Fs(cashTransaction.Amount));
                }

                report.AddRow("Gelir", Resources.Total.ToUpper(), "", Fs(it));
            }

            var totalCashIncome = cashIncomeTotal + ac.CashTotal + ReportContext.CurrentWorkPeriod.CashAmount;
            var totalCreditCardIncome = creditCardIncomeTotal + ac.CreditCardTotal + ReportContext.CurrentWorkPeriod.CreditCardAmount;
            var totalTicketIncome = ticketIncomeTotal + ac.TicketTotal + ReportContext.CurrentWorkPeriod.TicketAmount;

            report.AddBoldRow("Gelir", Resources.Income.ToUpper() + " " + Resources.Totals.ToUpper(), "", "");
            report.AddRow("Gelir", GetPaymentString(0), Resources.TotalIncome, Fs(totalCashIncome));
            report.AddRow("Gelir", GetPaymentString(1), Resources.TotalIncome, Fs(totalCreditCardIncome));
            report.AddRow("Gelir", GetPaymentString(2), Resources.TotalIncome, Fs(totalTicketIncome));
            report.AddRow("Gelir", Resources.GrandTotal.ToUpper(), "", Fs(totalCashIncome + totalCreditCardIncome + totalTicketIncome));

            //--------------------

            report.AddColumTextAlignment("Toplam", TextAlignment.Left, TextAlignment.Right);
            report.AddColumnLength("Toplam", "Auto", "25*");
            report.AddTable("Toplam", Resources.CashStatus, "");
            report.AddRow("Toplam", Resources.Cash, Fs(totalCashIncome - cashExpenseTotal));
            report.AddRow("Toplam", Resources.CreditCard, Fs(totalCreditCardIncome - creditCardExpenseTotal));
            report.AddRow("Toplam", Resources.Voucher, Fs(totalTicketIncome - ticketExpenseTotal));
            report.AddRow("Toplam", Resources.GrandTotal.ToUpper(),
                Fs((totalCashIncome - cashExpenseTotal) +
                (totalCreditCardIncome - creditCardExpenseTotal) +
                (totalTicketIncome - ticketExpenseTotal)));
            return report.Document;
        }

        private static string Fct(CashTransactionData data)
        {
            var cn = !string.IsNullOrEmpty(data.AccountName) ? data.AccountName + " " : "";
            return data.Date.ToShortDateString() + " " + cn + data.Name;
        }

        protected override string GetHeader()
        {
            return Resources.CashReport;
        }
    }
}
