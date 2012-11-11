using System.Windows.Documents;
using Samba.Localization.Properties;
using Samba.Presentation.Services;

namespace Samba.Modules.BasicReports.Reports.AccountReport
{
    class InternalAccountsViewModel : AccountReportViewModelBase
    {
        public InternalAccountsViewModel(IUserService userService, IApplicationState applicationState)
            : base(userService, applicationState)
        {
        }

        protected override FlowDocument GetReport()
        {
            return CreateReport(Resources.InternalAccounts, null, true);
        }

        protected override string GetHeader()
        {
            return Resources.InternalAccounts;
        }
    }
}
