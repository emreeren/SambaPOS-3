using System;
using System.ComponentModel.Composition;
using Samba.Domain.Models.Accounts;
using Samba.Services;

namespace Samba.Presentation.Services.Implementations
{
    [Export(typeof(IReportServiceClient))]
    class ReportServiceClient : IReportServiceClient
    {
        private readonly IReportService _reportService;
        private readonly IApplicationState _applicationState;

        [ImportingConstructor]
        public ReportServiceClient(IReportService reportService, IApplicationState applicationState)
        {
            _reportService = reportService;
            _applicationState = applicationState;
        }

        public void PrintAccountScreen(AccountScreen accountScreen)
        {
            _reportService.PrintAccountScreen(accountScreen, _applicationState.CurrentWorkPeriod, _applicationState.GetReportPrinter());
        }

        public void PrintAccountTransactions(Account account, string filter)
        {
            _reportService.PrintAccountTransactions(account, _applicationState.CurrentWorkPeriod, _applicationState.GetReportPrinter(), filter);
        }
    }
}