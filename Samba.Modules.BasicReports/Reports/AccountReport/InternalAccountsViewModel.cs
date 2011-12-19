using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Documents;
using Samba.Localization.Properties;
using Samba.Services;

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
