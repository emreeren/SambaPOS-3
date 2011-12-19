using System.Windows.Documents;
using Samba.Localization.Properties;
using Samba.Services;

namespace Samba.Modules.BasicReports.Reports.AccountReport
{
    class LiabilityReportViewModel : AccountReportViewModelBase
    {
        public LiabilityReportViewModel(IUserService userService, IApplicationState applicationState)
            : base(userService,applicationState)
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
