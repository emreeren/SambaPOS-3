using System.ComponentModel.Composition;
using Microsoft.Practices.Prism.MefExtensions.Modularity;
using Samba.Localization.Properties;
using Samba.Presentation.Common;

namespace Samba.Modules.PrinterModule
{
    [ModuleExport(typeof(PrinterModule))]
    class PrinterModule : ModuleBase
    {
        [ImportingConstructor]
        public PrinterModule()
        {
            AddDashboardCommand<PrinterListViewModel>(Resources.Printers, Resources.Settings, 10);
            AddDashboardCommand<PrintJobListViewModel>(Resources.PrintJobs, Resources.Settings, 10);
            AddDashboardCommand<PrinterTemplateCollectionViewModel>(Resources.PrinterTemplates, Resources.Settings, 10);
        }
    }
}
