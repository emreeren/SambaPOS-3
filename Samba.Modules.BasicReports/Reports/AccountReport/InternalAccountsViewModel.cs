using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Documents;
using Samba.Localization.Properties;

namespace Samba.Modules.BasicReports.Reports.AccountReport
{
    class InternalAccountsViewModel : AccountReportViewModelBase
    {
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
