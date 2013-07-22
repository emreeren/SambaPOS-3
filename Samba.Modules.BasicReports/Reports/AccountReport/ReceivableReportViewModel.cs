using System.Windows.Documents;
using Samba.Localization.Properties;
using Samba.Presentation.Services;
using Samba.Services;

namespace Samba.Modules.BasicReports.Reports.AccountReport
{
    public class ReceivableReportViewModel : AccountReportViewModelBase
    {
        public ReceivableReportViewModel(IUserService userService, IApplicationState applicationState, ILogService logService, ISettingService settingService)
            : base(userService, applicationState, logService, settingService)
        {
        }

        protected override FlowDocument GetReport()
        {
            return CreateReport(Resources.AccountsReceivable, true, false);
        }

        protected override string GetHeader()
        {
            return Resources.AccountsReceivable;
        }
    }
}
