using Samba.Domain.Models.Settings;
using Samba.Localization.Properties;
using Samba.Persistance.Data;
using Samba.Presentation.Common.ModelBase;

namespace Samba.Modules.PrinterModule
{
    public class PrinterListViewModel : EntityCollectionViewModelBase<PrinterViewModel, Printer>
    {
        protected override string CanDeleteItem(Printer model)
        {
            var count = Dao.Count<Terminal>(x => x.ReportPrinter.Id == model.Id || x.SlipReportPrinter.Id == model.Id);
            if (count > 0) return Resources.DeleteErrorPrinterAssignedToTerminal;
            count = Dao.Count<PrinterMap>(x => x.Printer.Id == model.Id);
            if (count > 0) return Resources.DeleteErrorPrinterAssignedToPrinterMap;
            return base.CanDeleteItem(model);
        }
    }
}
