using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using Samba.Localization.Properties;
using Samba.Presentation.Services.Common;
using Samba.Services.Common;

namespace Samba.Modules.BasicReports.ActionProcessors
{
    [Export(typeof(IActionType))]
    class PrintReport : ActionType
    {
        public override void Process(ActionData actionData)
        {
            var reportName = actionData.GetAsString("ReportName");
            if (!string.IsNullOrEmpty(reportName))
            {
                var report = ReportContext.Reports.FirstOrDefault(y => y.Header == reportName);
                if (report != null)
                {
                    ReportContext.CurrentWorkPeriod = ReportContext.ApplicationState.CurrentWorkPeriod;
                    var document = report.GetReportDocument();
                    ReportContext.PrinterService.PrintReport(document, ReportContext.ApplicationState.GetReportPrinter());
                }
            }
        }

        protected override object GetDefaultData()
        {
            return new { ReportName = "" };
        }

        protected override string GetActionName()
        {
            return Resources.PrintReport;
        }

        protected override string GetActionKey()
        {
            return ActionNames.PrintReport;
        }
    }
}
