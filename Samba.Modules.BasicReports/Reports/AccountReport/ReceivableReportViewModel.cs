using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Documents;
using Samba.Localization.Properties;
using Samba.Services;

namespace Samba.Modules.BasicReports.Reports.AccountReport
{
    public class ReceivableReportViewModel : AccountReportViewModelBase
    {
        public ReceivableReportViewModel(IUserService userService, IApplicationState applicationState)
            : base(userService, applicationState)
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
