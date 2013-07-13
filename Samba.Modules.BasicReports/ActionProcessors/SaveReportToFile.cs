using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using Samba.Localization.Properties;
using Samba.Services.Common;

namespace Samba.Modules.BasicReports.ActionProcessors
{
    [Export(typeof(IActionType))]
    class SaveReportToFile : ActionType
    {
        public override void Process(ActionData actionData)
        {
            var reportName = actionData.GetAsString("ReportName");
            var fileName = actionData.GetAsString("FileName");
            if (!string.IsNullOrEmpty(reportName))
            {
                var report = ReportContext.Reports.FirstOrDefault(y => y.Header == reportName);
                if (report != null)
                {
                    ReportContext.CurrentWorkPeriod = ReportContext.ApplicationState.CurrentWorkPeriod;
                    var document = report.GetReportDocument();
                    ReportViewModelBase.SaveAsXps(fileName, document);
                }
            }
        }

        protected override object GetDefaultData()
        {
            return new { ReportName = "", FileName = "" };
        }

        protected override string GetActionName()
        {
            return Resources.SaveReportToFile;
        }

        protected override string GetActionKey()
        {
            return "SaveReportToFile";
        }
    }
}
