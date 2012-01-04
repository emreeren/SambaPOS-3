using System.ComponentModel.Composition;
using Microsoft.Practices.Prism.MefExtensions.Modularity;
using Samba.Domain.Models.Settings;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Presentation.Common.ModelBase;

namespace Samba.Modules.PrinterModule
{
    [ModuleExport(typeof(PrinterModule))]
    class PrinterModule : ModuleBase
    {
        [ImportingConstructor]
        public PrinterModule()
        {
            AddDashboardCommand<EntityCollectionViewModelBase<PrinterViewModel, Printer>>(Resources.Printers, Resources.Settings, 10);
            AddDashboardCommand<EntityCollectionViewModelBase<PrintJobViewModel, PrintJob>>(Resources.PrintJobs, Resources.Settings, 10);
            AddDashboardCommand<EntityCollectionViewModelBase<PrinterTemplateViewModel, PrinterTemplate>>(Resources.PrinterTemplates, Resources.Settings, 10);
        }
    }
}
