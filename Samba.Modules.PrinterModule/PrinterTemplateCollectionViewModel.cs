using System.ComponentModel.Composition;
using Samba.Domain.Models.Settings;
using Samba.Presentation.Common.ModelBase;

namespace Samba.Modules.PrinterModule
{
    [Export, PartCreationPolicy(CreationPolicy.NonShared)]
    public class PrinterTemplateCollectionViewModel : EntityCollectionViewModelBase<PrinterTemplateViewModel, PrinterTemplate>
    {
    }
}
