using System.Windows.Documents;
using Samba.Localization.Properties;
using Samba.Presentation.Services;
using Samba.Services;

namespace Samba.Modules.BasicReports.Reports.AccountReport
{
    class LiabilityReportViewModel : AccountReportViewModelBase
    {
        public LiabilityReportViewModel(IUserService userService, IApplicationState applicationState, ILogService logService)
            : base(userService, applicationState, logService)
        {
        }

        protected override FlowDocument GetReport()
        {
            return CreateReport(Resources.AccountsLiability, false, false);
        }

        protected override string GetHeader()
        {
            return Resources.AccountsLiability;
        }
    }
}
