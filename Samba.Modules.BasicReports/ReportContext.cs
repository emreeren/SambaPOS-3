using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Inventory;
using Samba.Domain.Models.Menus;
using Samba.Domain.Models.Settings;
using Samba.Domain.Models.Tickets;
using Samba.Localization.Properties;
using Samba.Modules.BasicReports.Reports;
using Samba.Modules.BasicReports.Reports.AccountReport;
using Samba.Modules.BasicReports.Reports.CSVBuilder;
using Samba.Modules.BasicReports.Reports.EndOfDayReport;
using Samba.Modules.BasicReports.Reports.InventoryReports;
using Samba.Modules.BasicReports.Reports.ProductReport;
using Samba.Persistance.Data;
using Samba.Presentation.Services;
using Samba.Services;

namespace Samba.Modules.BasicReports
{
    public static class ReportContext
    {
        public static IWorkPeriodService WorkPeriodService { get; set; }
        public static IInventoryService InventoryService { get; set; }
        public static IPrinterService PrinterService { get; set; }
        public static IUserService UserService { get; set; }
        public static IApplicationState ApplicationState { get; set; }
        public static ILogService LogService { get; set; }
        public static ICacheService CacheService { get; set; }
        public static ISettingService SettingService { get; set; }

        private static IList<ReportViewModelBase> _reports;
        public static IList<ReportViewModelBase> Reports
        {
            get { return _reports ?? (_reports = GetReports()); }
        }

        private static IList<ReportViewModelBase> GetReports()
        {
            return new List<ReportViewModelBase>
                          {
                              new EndDayReportViewModel(UserService,ApplicationState,LogService,SettingService,CacheService),
                              new ProductReportViewModel(UserService,ApplicationState,LogService,SettingService,CacheService),
                              new LiabilityReportViewModel(UserService,ApplicationState,LogService,SettingService),
                              new ReceivableReportViewModel(UserService,ApplicationState,LogService,SettingService),
                              new InternalAccountsViewModel(UserService,ApplicationState,LogService,SettingService),
                              new PurchaseReportViewModel(UserService,ApplicationState,LogService,SettingService),
                              new InventoryReportViewModel(UserService,ApplicationState,CacheService,LogService,SettingService),
                              new CostReportViewModel(UserService,ApplicationState,LogService,SettingService),
                              new CsvBuilderViewModel(UserService,ApplicationState,LogService,SettingService)
                          };
        }

        private static IEnumerable<PaymentType> _paymentTypes;
        public static IEnumerable<PaymentType> PaymentTypes
        {
            get { return _paymentTypes ?? (_paymentTypes = Dao.Query<PaymentType>()); }
        }

        private static IEnumerable<Ticket> _tickets;
        public static IEnumerable<Ticket> Tickets { get { return _tickets ?? (_tickets = GetTickets()); } }

        private static IEnumerable<TicketType> _ticketTypes;
        public static IEnumerable<TicketType> TicketTypes { get { return _ticketTypes ?? (_ticketTypes = GetTicketTypes()); } }

        private static IEnumerable<MenuItem> _menutItems;
        public static IEnumerable<MenuItem> MenuItems { get { return _menutItems ?? (_menutItems = GetMenuItems()); } }

        private static IEnumerable<InventoryTransactionDocument> _transactions;
        public static IEnumerable<InventoryTransactionDocument> Transactions { get { return _transactions ?? (_transactions = GetTransactions()); } }

        private static IEnumerable<PeriodicConsumption> _periodicConsumptions;
        public static IEnumerable<PeriodicConsumption> PeriodicConsumptions { get { return _periodicConsumptions ?? (_periodicConsumptions = GetPeriodicConsumtions()); } }

        private static IEnumerable<InventoryItem> _inventoryItems;
        public static IEnumerable<InventoryItem> InventoryItems { get { return _inventoryItems ?? (_inventoryItems = GetInventoryItems()); } }

        private static IEnumerable<TicketTagGroup> _ticketTagGroups;
        public static IEnumerable<TicketTagGroup> TicketTagGroups
        {
            get { return _ticketTagGroups ?? (_ticketTagGroups = Dao.Query<TicketTagGroup>()); }
        }

        private static IEnumerable<WorkPeriod> _workPeriods;
        public static IEnumerable<WorkPeriod> WorkPeriods
        {
            get { return _workPeriods ?? (_workPeriods = Dao.Query<WorkPeriod>()); }
        }

        private static IEnumerable<CalculationType> _calculationTypes;
        public static IEnumerable<CalculationType> CalculationTypes
        {
            get { return _calculationTypes ?? (_calculationTypes = Dao.Query<CalculationType>()); }
        }

        private static IEnumerable<TaxTemplate> _taxTemplates;
        public static IEnumerable<TaxTemplate> TaxTemplates
        {
            get { return _taxTemplates ?? (_taxTemplates = Dao.Query<TaxTemplate>(x => x.AccountTransactionType)); }
        }

        private static IEnumerable<AccountTransactionValue> _accountTransactionValues;
        public static IEnumerable<AccountTransactionValue> AccountTransactionValues
        {
            get { return _accountTransactionValues ?? (_accountTransactionValues = GetAccountTransactionValues()); }
        }

        private static WorkPeriod _currentWorkPeriod;
        public static WorkPeriod CurrentWorkPeriod
        {
            get { return _currentWorkPeriod ?? (_currentWorkPeriod = ApplicationState.CurrentWorkPeriod); }
            set
            {
                _currentWorkPeriod = value;
                _tickets = null;
                _periodicConsumptions = null;
                _transactions = null;
                StartDate = CurrentWorkPeriod.StartDate;
                EndDate = CurrentWorkPeriod.EndDate;
                if (StartDate == EndDate) EndDate = DateTime.Now;
            }
        }

        public static DateTime StartDate { get; set; }
        public static DateTime EndDate { get; set; }

        public static string StartDateString { get { return StartDate.ToString("dd MM yyyy"); } set { StartDate = StrToDate(value); } }
        public static string EndDateString { get { return EndDate.ToString("dd MM yyyy"); } set { EndDate = StrToDate(value); } }

        private static DateTime StrToDate(string value)
        {
            var vals = value.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Select(x => Convert.ToInt32(x)).ToList();
            if (vals.Count == 1) vals.Add(DateTime.Now.Month);
            if (vals.Count == 2) vals.Add(DateTime.Now.Year);

            if (vals[2] < 1) { vals[2] = DateTime.Now.Year; }
            if (vals[2] < 1000) { vals[2] += 2000; }

            if (vals[1] < 1) { vals[1] = 1; }
            if (vals[1] > 12) { vals[1] = 12; }

            var dim = DateTime.DaysInMonth(vals[0], vals[1]);
            if (vals[0] < 1) { vals[0] = 1; }
            if (vals[0] > dim) { vals[0] = dim; }
            return new DateTime(vals[2], vals[1], vals[0]);
        }

        private static IEnumerable<InventoryItem> GetInventoryItems()
        {
            return Dao.Query<InventoryItem>();
        }

        private static IEnumerable<InventoryTransactionDocument> GetTransactions()
        {
            if (CurrentWorkPeriod.StartDate != CurrentWorkPeriod.EndDate)
                return Dao.Query<InventoryTransactionDocument>(x => x.Date >= CurrentWorkPeriod.StartDate && x.Date < CurrentWorkPeriod.EndDate, x => x.TransactionItems, x => x.TransactionItems.Select(y => y.InventoryItem));
            return Dao.Query<InventoryTransactionDocument>(x => x.Date >= CurrentWorkPeriod.StartDate, x => x.TransactionItems, x => x.TransactionItems.Select(y => y.InventoryItem));
        }

        private static IEnumerable<PeriodicConsumption> GetPeriodicConsumtions()
        {
            if (CurrentWorkPeriod.StartDate != CurrentWorkPeriod.EndDate)
                return Dao.Query<PeriodicConsumption>(x => x.StartDate >= CurrentWorkPeriod.StartDate && x.EndDate <= CurrentWorkPeriod.EndDate, x => x.WarehouseConsumptions.Select(y => y.CostItems), x => x.WarehouseConsumptions.Select(y => y.PeriodicConsumptionItems));
            return Dao.Query<PeriodicConsumption>(x => x.StartDate >= CurrentWorkPeriod.StartDate, x => x.WarehouseConsumptions.Select(y => y.CostItems), x => x.WarehouseConsumptions.Select(y => y.PeriodicConsumptionItems));
        }

        private static IEnumerable<MenuItem> GetMenuItems()
        {
            return Dao.Query<MenuItem>();
        }

        private static IEnumerable<TicketType> GetTicketTypes()
        {
            return Dao.Query<TicketType>();
        }

        private static IEnumerable<Ticket> GetTickets()
        {
            if (CurrentWorkPeriod.StartDate == CurrentWorkPeriod.EndDate)
                return Dao.Query<Ticket>(
                    x => x.LastPaymentDate >= CurrentWorkPeriod.StartDate,
                    x => x.TransactionDocument.AccountTransactions,
                    x => x.Payments.Select(y => y.AccountTransaction), x => x.Calculations, x => x.Orders.Select(y => y.ProductTimerValue));

            return Dao.Query<Ticket>(
                    x => x.LastPaymentDate >= CurrentWorkPeriod.StartDate && x.LastPaymentDate < CurrentWorkPeriod.EndDate,
                    x => x.TransactionDocument.AccountTransactions,
                    x => x.Payments.Select(y => y.AccountTransaction), x => x.Calculations, x => x.Orders.Select(y => y.ProductTimerValue));
        }

        private static IEnumerable<AccountTransactionValue> GetAccountTransactionValues()
        {
            if (CurrentWorkPeriod.StartDate == CurrentWorkPeriod.EndDate)
                return Dao.Query<AccountTransactionValue>(x => x.Date >= CurrentWorkPeriod.StartDate);
            return
                Dao.Query<AccountTransactionValue>(x => x.Date >= CurrentWorkPeriod.StartDate && x.Date < CurrentWorkPeriod.EndDate);
        }

        public static string CurrencyFormat { get { return "#,#0.00;-#,#0.00;-"; } }

        public static void ResetCache()
        {
            _tickets = null;
            _transactions = null;
            _accountTransactionValues = null;
            _periodicConsumptions = null;
            _currentWorkPeriod = null;
            _thisMonthWorkPeriod = null;
            _lastMonthWorkPeriod = null;
            _thisWeekWorkPeriod = null;
            _lastWeekWorkPeriod = null;
            _yesterdayWorkPeriod = null;
            _todayWorkPeriod = null;
            _workPeriods = null;
            _calculationTypes = null;
            _paymentTypes = null;
            _taxTemplates = null;
        }

        private static WorkPeriod _thisMonthWorkPeriod;
        public static WorkPeriod ThisMonthWorkPeriod
        {
            get { return _thisMonthWorkPeriod ?? (_thisMonthWorkPeriod = CreateThisMonthWorkPeriod()); }
        }
        private static WorkPeriod _lastMonthWorkPeriod;
        public static WorkPeriod LastMonthWorkPeriod
        {
            get { return _lastMonthWorkPeriod ?? (_lastMonthWorkPeriod = CreateLastMonthWorkPeriod()); }
        }

        private static WorkPeriod _thisWeekWorkPeriod;
        public static WorkPeriod ThisWeekWorkPeriod
        {
            get { return _thisWeekWorkPeriod ?? (_thisWeekWorkPeriod = CreateThisWeekWorkPeriod()); }
        }

        private static WorkPeriod _lastWeekWorkPeriod;
        public static WorkPeriod LastWeekWorkPeriod
        {
            get { return _lastWeekWorkPeriod ?? (_lastWeekWorkPeriod = CreateLastWeekWorkPeriod()); }
        }

        private static WorkPeriod _yesterdayWorkPeriod;
        public static WorkPeriod YesterdayWorkPeriod
        {
            get { return _yesterdayWorkPeriod ?? (_yesterdayWorkPeriod = CreateYesterdayWorkPeriod()); }
        }

        private static WorkPeriod _todayWorkPeriod;
        public static WorkPeriod TodayWorkPeriod
        {
            get { return _todayWorkPeriod ?? (_todayWorkPeriod = CreteTodayWorkPeriod()); }
        }

        private static WorkPeriod CreteTodayWorkPeriod()
        {
            var start = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
            return CreateCustomWorkPeriod(Resources.Today, start, start.AddDays(1).AddSeconds(-1));
        }

        private static WorkPeriod CreateYesterdayWorkPeriod()
        {
            var start = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day).AddDays(-1);
            var end = start.AddDays(1).AddSeconds(-1);
            return CreateCustomWorkPeriod(Resources.Yesterday, start, end);
        }

        private static WorkPeriod CreateLastMonthWorkPeriod()
        {
            var lastmonth = DateTime.Now.AddMonths(-1);
            var start = new DateTime(lastmonth.Year, lastmonth.Month, 1);
            var end = new DateTime(lastmonth.Year, lastmonth.Month, DateTime.DaysInMonth(lastmonth.Year, lastmonth.Month));
            end = end.AddDays(1).AddSeconds(-1);
            return CreateCustomWorkPeriod(Resources.PastMonth + ": " + CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(start.Month), start, end);
        }

        private static WorkPeriod CreateThisMonthWorkPeriod()
        {
            var start = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var end = DateTime.Now.Date.AddDays(1).AddSeconds(-1);
            return CreateCustomWorkPeriod(Resources.ThisMonth + ": " + CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(start.Month), start, end);
        }

        private static WorkPeriod CreateThisWeekWorkPeriod()
        {
            var w = (int)DateTime.Now.DayOfWeek;
            var start = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day).AddDays(-w + 1);
            var end = DateTime.Now.Date.AddDays(1).AddSeconds(-1);
            return CreateCustomWorkPeriod(Resources.ThisWeek, start, end);
        }

        private static WorkPeriod CreateLastWeekWorkPeriod()
        {
            var w = (int)DateTime.Now.DayOfWeek;
            var start = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day).AddDays(-6 - w);
            var end = start.AddDays(7).AddSeconds(-1);
            return CreateCustomWorkPeriod(Resources.PastWeek, start, end);
        }

        public static IEnumerable<WorkPeriod> GetWorkPeriods(DateTime startDate, DateTime endDate)
        {
            var wp = WorkPeriods.Where(x => x.EndDate >= endDate && x.StartDate < startDate);
            if (!wp.Any())
                wp = WorkPeriods.Where(x => x.StartDate >= startDate && x.StartDate < endDate);
            if (!wp.Any())
                wp = WorkPeriods.Where(x => x.EndDate >= startDate && x.EndDate < endDate);
            if (!wp.Any() && ApplicationState.CurrentWorkPeriod.StartDate < startDate)
                wp = new List<WorkPeriod> { ApplicationState.CurrentWorkPeriod };
            return wp.OrderBy(x => x.StartDate);
        }

        public static WorkPeriod CreateCustomWorkPeriod(string name, DateTime startDate, DateTime endDate)
        {
            var periods = GetWorkPeriods(startDate, endDate).ToList();
            var startPeriod = periods.FirstOrDefault();
            var endPeriod = periods.LastOrDefault();
            var start = startPeriod != null ? startPeriod.StartDate : startDate;
            var end = endPeriod != null ? endPeriod.EndDate : endDate;
            if (endPeriod != null && end == endPeriod.StartDate)
                end = DateTime.Now;
            var result = new WorkPeriod { Name = name, StartDate = start, EndDate = end };
            return result;
        }

        public static string GetUserName(int userId)
        {
            return UserService.ContainsUser(userId) ? UserService.GetUserName(userId) : Resources.UndefinedWithBrackets;
        }

        internal static AmountCalculator GetIncomeCalculator()
        {
            var groups = Tickets
                .SelectMany(x => x.Payments)
                .Where(x => x.Amount >= 0)
                .GroupBy(x => x.PaymentTypeId)
                .Select(x => new TenderedAmount { PaymentName = GetPaymentTypeName(x.Key), Amount = x.Sum(y => y.Amount) });
            return new AmountCalculator(groups);
        }
        internal static AmountCalculator GetIncomeCalculatorByUser(int userId)
        {
            var groups = Tickets
                .SelectMany(x => x.Payments.Where(y => y.UserId == userId))
                .Where(x => x.Amount >= 0)
                .GroupBy(x => x.PaymentTypeId)
                .Select(x => new TenderedAmount { PaymentName = GetPaymentTypeName(x.Key), Amount = x.Sum(y => y.Amount) });
            return new AmountCalculator(groups);
        }

        internal static AmountCalculator GetRefundCalculator()
        {
            var groups = Tickets
                .SelectMany(x => x.Payments)
                .Where(x => x.Amount < 0)
                .GroupBy(x => x.PaymentTypeId)
                .Select(x => new TenderedAmount { PaymentName = GetPaymentTypeName(x.Key), Amount = x.Sum(y => y.Amount) });
            return new AmountCalculator(groups);
        }

        private static string GetPaymentTypeName(int paymentTypeId)
        {
            var pt = PaymentTypes.SingleOrDefault(x => x.Id == paymentTypeId);
            return pt != null ? pt.Name : "";
        }

        public static PeriodicConsumption GetCurrentPeriodicConsumption()
        {
            return InventoryService.GetCurrentPeriodicConsumption();
        }
    }
}
