using System.ComponentModel.Composition;
using System.Xml;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using Microsoft.Practices.Prism.MefExtensions.Modularity;
using Samba.Domain.Models.Settings;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Presentation.Common.ModelBase;
using Samba.Presentation.Services.Common;

namespace Samba.Modules.PrinterModule
{
    [ModuleExport(typeof(PrinterModule))]
    class PrinterModule : ModuleBase
    {
        [ImportingConstructor]
        public PrinterModule()
        {
            AddDashboardCommand<EntityCollectionViewModelBase<PrinterViewModel, Printer>>(Resources.Printer.ToPlural(), Resources.Settings, 20);
            AddDashboardCommand<EntityCollectionViewModelBase<PrintJobViewModel, PrintJob>>(Resources.PrintJob.ToPlural(), Resources.Settings, 20);
            AddDashboardCommand<EntityCollectionViewModelBase<PrinterTemplateViewModel, PrinterTemplate>>(Resources.PrinterTemplate.ToPlural(), Resources.Settings, 20);

            HighlightingManager.Instance.RegisterHighlighting("Template", null, () => LoadHighlightingDefinition("Template.xshd"));
        }

        public static IHighlightingDefinition LoadHighlightingDefinition(string resourceName)
        {
            var type = typeof(PrinterModule);
            var fullName = type.Namespace + "." + resourceName;
            using (var stream = type.Assembly.GetManifestResourceStream(fullName))
            using (var reader = new XmlTextReader(stream))
                return HighlightingLoader.Load(reader, HighlightingManager.Instance);
        }
    }
}
